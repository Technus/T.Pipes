using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Pipes.Test
{
  [ExcludeFromCodeCoverage]
  public class PipePipeTests
  {
    [Fact]
    public void PipeClientNames()
    {
      var pipe = Substitute.For<H.Pipes.IPipeClient<PipeMessage>>();
      pipe.PipeName.Returns(Guid.NewGuid().ToString());
      pipe.ServerName.Returns(Guid.NewGuid().ToString());
      using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      using var client = new PipeClient<H.Pipes.IPipeClient<PipeMessage>, PipeMessage, IPipeCallback<PipeMessage>>(pipe, clientCallback);

      client.ServerName.Should().Be(pipe.ServerName);
      client.PipeName.Should().Be(pipe.PipeName);
    }

    [Fact]
    public void PipeServerNames()
    {
      var pipe = Substitute.For<H.Pipes.IPipeServer<PipeMessage>>();
      pipe.PipeName.Returns(Guid.NewGuid().ToString());
      using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      using var server = new PipeServer<H.Pipes.IPipeServer<PipeMessage>, PipeMessage, IPipeCallback<PipeMessage>>(pipe, serverCallback);

      server.ServerName.Should().Be(pipe.PipeName);
      server.PipeName.Should().Be(pipe.PipeName);
    }

    [Fact]
    public void PipeClientDispose()
    {
      var pipe = Substitute.For<H.Pipes.IPipeClient<PipeMessage>>();
      using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      var client = new PipeClient<H.Pipes.IPipeClient<PipeMessage>, PipeMessage, IPipeCallback<PipeMessage>>(pipe, clientCallback);

      var task = client.Dispose;

      task.Should().NotThrow();
    }

    [Fact]
    public void PipeServerDispose()
    {
      var pipe = Substitute.For<H.Pipes.IPipeServer<PipeMessage>>();
      using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      var server = new PipeServer<H.Pipes.IPipeServer<PipeMessage>, PipeMessage, IPipeCallback<PipeMessage>>(pipe, serverCallback);

      var task = server.Dispose;

      task.Should().NotThrow();
    }

    [Fact]
    public async Task PipeClientDisposeAsync()
    {
      var pipe = Substitute.For<H.Pipes.IPipeClient<PipeMessage>>();
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      var client = new PipeClient<H.Pipes.IPipeClient<PipeMessage>, PipeMessage, IPipeCallback<PipeMessage>>(pipe, clientCallback);

      var task = client.DisposeAsync();

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public async Task PipeServerDisposeAsync()
    {
      var pipe = Substitute.For<H.Pipes.IPipeServer<PipeMessage>>();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();
      var server = new PipeServer<H.Pipes.IPipeServer<PipeMessage>, PipeMessage, IPipeCallback<PipeMessage>>(pipe, serverCallback);

      var task = server.DisposeAsync();

      await task.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public async Task PipeClientProperPipe()
    {
      var pipe = Substitute.For<H.Pipes.IPipeClient<PipeMessage>>();
      await using var clientCallback = Substitute.For<IPipeCallback<PipeMessage>>();

      await using var client = new PipeClient<H.Pipes.IPipeClient<PipeMessage>, PipeMessage,IPipeCallback<PipeMessage>>(pipe, clientCallback);

      client.Pipe.Should().Be(pipe);
    }

    [Fact]
    public async Task PipeServerProperPipe()
    {
      var pipe = Substitute.For<H.Pipes.IPipeServer<PipeMessage>>();
      await using var serverCallback = Substitute.For<IPipeCallback<PipeMessage>>();

      await using var server = new PipeServer<H.Pipes.IPipeServer<PipeMessage>, PipeMessage, IPipeCallback<PipeMessage>>(pipe, serverCallback);

      server.Pipe.Should().Be(pipe);
    }
  }
}
