using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Pipes.Test
{
  public class PipeCallbackTests
  {
    [Fact]
    public async Task PipeClientProperCallback()
    {
      var pipeName = Guid.NewGuid().ToString();

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<IPipeCallback<PipeMessage>>(pipeName, clientCallback);

      client.Callback.Should().Be(clientCallback);

      (client as IPipeConnection<PipeMessage>).Callback.Should().Be(clientCallback);
    }

    [Fact]
    public async Task PipeServerProperCallback()
    {
      var pipeName = Guid.NewGuid().ToString();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<IPipeCallback<PipeMessage>>(pipeName, serverCallback);

      server.Callback.Should().Be(serverCallback);

      (server as IPipeConnection<PipeMessage>).Callback.Should().Be(serverCallback);
    }
  }
}
