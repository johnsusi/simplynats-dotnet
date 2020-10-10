using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SimplyNats.Exceptions;
using SimplyNats.Test.Helpers;
using Xunit;

namespace SimplyNats.Test.IntegrationTests
{
    public class ConnectionTest : IntegrationTest
    {

        [Fact]
        public async Task Connect_WithInvalidPort_ShouldCompleteWithException()
        {
          var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
          socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
          var port = ((IPEndPoint)socket.LocalEndPoint).Port;

          var connection = new Connection(port);
          await Assert.ThrowsAsync<ConnectionFailed>(async () => await connection.Completion);


          try
          {
            await connection.DisposeAsync();
          }
          catch(ConnectionFailed)
          {
          }

        }

        [Fact]
        public async Task ShouldConnect()
        {
          await using var nats = await Up("nats");
          var connection = new Connection(nats[4222]);
          var exception = await Record.ExceptionAsync(async () => await connection.Connected);
          Assert.Null(exception);
        }


        [Fact]
        public async Task Publish_WithNullAction_ShouldThrow()
        {
          await using var nats = await Up("nats");
          await using var connection = new Connection(nats[4222]);
          await Assert.ThrowsAsync<ArgumentNullException>(async () => await connection.Publish(null));
        }

    }
}
