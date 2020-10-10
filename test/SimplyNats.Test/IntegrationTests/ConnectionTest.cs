using System;
using System.Threading.Tasks;
using SimplyNats.Test.Helpers;
using Xunit;

namespace SimplyNats.Test.IntegrationTests
{
    public class ConnectionTest : IntegrationTest
    {
        [Fact]
        public async Task ShouldConnect()
        {

          await using var nats = await Up("nats");

          var connection = new Connection(nats[4222]);

          var exception = await Record.ExceptionAsync(async () => await connection.Connected );

          Assert.Null(exception);

        }
    }
}
