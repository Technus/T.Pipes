using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Pipes.Test
{
  public class PipeTests
  {
    [Fact]
    public async Task PipeClientProperPipe()
    {
      var pipe = Substitute.For<H.Pipes.IPipeClient<PipeMessage>>();

      var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var client = new PipeClient<H.Pipes.IPipeClient<PipeMessage>, PipeMessage,IPipeCallback<PipeMessage>>(pipe, clientCallback);

      client.Pipe.Should().Be(pipe);
    }

    [Fact]
    public async Task PipeServerProperPipe()
    {
      var pipe = Substitute.For<H.Pipes.IPipeServer<PipeMessage>>();

      var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      await using var server = new PipeServer<H.Pipes.IPipeServer<PipeMessage>, PipeMessage, IPipeCallback<PipeMessage>>(pipe, serverCallback);

      server.Pipe.Should().Be(pipe);
    }
  }
}
