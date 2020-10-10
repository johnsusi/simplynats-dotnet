using System;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Xunit;

namespace SimplyNats.Test.Helpers
{


  public class Container : IAsyncDisposable, IDisposable
  {

    private DockerClient _client;
    private string _id;
    private ContainerInspectResponse _info;

    public int this[int port]
    {
      get
      {
        var ports = _info.NetworkSettings.Ports[$"{port}/tcp"];
        if (ports.Count == 0) throw new Exception($"No mapping exist for port {port}");
        var mapping = ports[0].HostPort;
        if (!int.TryParse(mapping, out var result))
          throw new Exception($"No mapping exist for port {port}");
        return result;
      }
    }

    public Container(DockerClient client, string id, ContainerInspectResponse info)
    {
      _client = client;
      _id = id;
      _info = info;
    }

    public void Dispose()
    {
    }

    public async ValueTask DisposeAsync()
    {
      await _client.Containers.StopContainerAsync(_id, new ContainerStopParameters
      {
      });
      Dispose();
    }
  }

  [Trait("Category","Integration")]
  public class IntegrationTest
  {

    public async Task<Container> Up(string image)
    {

      DockerClient client = new DockerClientConfiguration()
        .CreateClient();


      await client.Images
          .CreateImageAsync(new ImagesCreateParameters
          {
            FromImage = image,
            Tag = "latest"
          },
              new AuthConfig(),
              new Progress<JSONMessage>());


      var response = await client.Containers.CreateContainerAsync(new CreateContainerParameters
      {
        Image = image,
        HostConfig = new HostConfig
        {
          PublishAllPorts = true
        }
      });

      await client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters
      {
      });

      var info = await client.Containers.InspectContainerAsync(response.ID);
      return new Container(client, response.ID, info);
    }
  }
}