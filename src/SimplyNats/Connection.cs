using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text.Encodings;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SimplyNats.Exceptions;
using SimplyNats.Protocol;

namespace SimplyNats
{
  public class Connection : IAsyncDisposable, IDisposable
  {

    enum State { NotConnected, Connecting, Connected, ReadCommand, ReadPayload, Completed, Disposed }



    public class Options
    {
      public Uri NatsUrl = new Uri("nats://localhost:4222");
    }

    private readonly Options _options;
    private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
    private readonly Task _task;
    public Task Completion => _task;
    private readonly TaskCompletionSource<bool> _connected = new TaskCompletionSource<bool>();
    private State state = State.NotConnected;


    internal class Job
    {
      internal Action<Publisher> Action { get; }
      internal TaskCompletionSource<bool> Completion { get; }
      internal Job(Action<Publisher> action, TaskCompletionSource<bool>? completion = null)
      {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Completion = completion ?? new TaskCompletionSource<bool>();
      }
    }
    private readonly Channel<Job> _jobs = Channel.CreateUnbounded<Job>();
    public Task Connected => _connected.Task;
    public Connection(int port) : this(new Options { NatsUrl = new Uri($"nats://localhost:{port}") })
    {
    }

    public Connection(Options options)
    {
      _options = options;
      _handler = HandleConnecting;
      _task = ExecuteAsync(_cancel.Token);
    }

    public struct Publisher
    {
      private readonly PipeWriter _writer;
      internal Publisher(PipeWriter writer)
      {
        _writer = writer;
      }

      public void Publish(ref ReadOnlySpan<byte> buffer)
      {
        var memory = _writer.GetSpan((int)buffer.Length);
        buffer.CopyTo(memory);
      }
    }

    public Task Publish(Action<Publisher> action)
    {
      var job = new Job(action);
      _jobs.Writer.TryWrite(job);
      return job.Completion.Task;
    }

    private async Task ProcessJobQueue(PipeWriter writer, CancellationToken cancellationToken)
    {

      while (!cancellationToken.IsCancellationRequested)
      {

        var publisher = new Publisher(writer);

        while (_jobs.Reader.TryRead(out var job))
        {
          job.Action(publisher);
          job.Completion.SetResult(true);
        }

        var result = await writer.FlushAsync(cancellationToken);
        if (result.IsCompleted) break;
        if (!await _jobs.Reader.WaitToReadAsync(cancellationToken)) break;
      }

      writer.Complete();

    }


    protected async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      state = State.Connecting;
      var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

      try
      {
        await socket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, _options.NatsUrl.Port));
      }
      catch(Exception err)
      {
        throw new ConnectionFailed(err);      
      }
      
      var stream = new NetworkStream(socket);
      var reader = PipeReader.Create(stream);
      var writer = PipeWriter.Create(stream);

      var _jobTask = ProcessJobQueue(writer, cancellationToken);

      while (!cancellationToken.IsCancellationRequested)
      {
        var result = await reader.ReadAsync();
        var buffer = result.Buffer;

        while (TryReadLine(ref buffer, out var line))
        {
          var command = System.Text.Encoding.UTF8.GetString(line.ToArray<byte>());
          state = await _handler(command);
          var consumed = buffer.GetPosition(line.Length + 2);
          reader.AdvanceTo(consumed, buffer.End);
          await Task.Delay(1000);
          if (result.IsCompleted) break;
        }


        if (result.IsCompleted) break;
      }
      await reader.CompleteAsync();
    }


    private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
      var reader = new SequenceReader<byte>(buffer);
      var delim = new [] { (byte)'\r', (byte)'\n' };
      return reader.TryReadTo(out line, delim, advancePastDelimiter: true);
    }

    public async ValueTask DisposeAsync()
    {
      _cancel.Cancel();
      await Completion;
      Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (state != State.Disposed)
      {
        if (disposing)
        {
          // TODO: dispose managed state (managed objects)
          _cancel.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        state = State.Disposed;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Connection()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }


    private Info? _info = null;
    private Func<string, ValueTask<State>> _handler;
    private async ValueTask<State> HandleConnecting(string command)
    {
      if (command.StartsWith("INFO "))
      {
        var json = command.Substring(5);
        _info = Info.FromJson(json);
        var options = new Connect
        {
        };
        await Publish(p =>
        {
          var connect = new ReadOnlySpan<byte>(System.Text.Encoding.ASCII.GetBytes($"CONNECT {options.ToJson()}"));
          p.Publish(ref connect);
        });
        _connected.SetResult(true);
        return State.Connected;
      }
      else return state;
    }

    private ValueTask<State> HandleConnected(string command)
    {
      return new ValueTask<State>(State.Connected);
    }
  }
}
