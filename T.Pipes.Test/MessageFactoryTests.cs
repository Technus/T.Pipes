using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Pipes.Test
{
  [ExcludeFromCodeCoverage]
  public class MessageFactoryTests
  {
    [Fact]
    public void MakesCommand()
    {
      var cmd = Guid.NewGuid().ToString();
      var dut = PipeMessageFactory.Instance;

      var result = dut.CreateCommand(cmd);

      result.Command.Should().Be(cmd);
      result.PacketType.Should().Be(PacketType.Command);
      result.Parameter.Should().BeNull();

      var result2 = dut.CreateCommand(cmd);

      result2.Command.Should().Be(cmd);
      result2.PacketType.Should().Be(PacketType.Command);
      result2.Parameter.Should().BeNull();

      result.Id.Should().NotBe(result2.Id);
    }

    [Fact]
    public void MakesCommandWithParameter()
    {
      var cmd = Guid.NewGuid().ToString();
      var param = Guid.NewGuid();
      var dut = PipeMessageFactory.Instance;

      var result = dut.CreateCommand(cmd, param);

      result.Command.Should().Be(cmd);
      result.PacketType.Should().Be(PacketType.Command);
      result.Parameter.Should().Be(param);

      param = Guid.NewGuid();

      var result2 = dut.CreateCommand(cmd, param);

      result2.Command.Should().Be(cmd);
      result2.PacketType.Should().Be(PacketType.Command);
      result2.Parameter.Should().Be(param);

      result.Id.Should().NotBe(result2.Id);
    }

    [Fact]
    public void MakesCommandCancellation()
    {
      var cmd = Guid.NewGuid().ToString();
      var dut = PipeMessageFactory.Instance;

      var result = dut.CreateCommandCancellation(cmd);

      result.Command.Should().Be(cmd);
      result.PacketType.Should().Be(PacketType.CommandCancellation);
      result.Parameter.Should().BeNull();

      var result2 = dut.CreateCommandCancellation(cmd);

      result2.Command.Should().Be(cmd);
      result2.PacketType.Should().Be(PacketType.CommandCancellation);
      result2.Parameter.Should().BeNull();

      result.Id.Should().NotBe(result2.Id);
    }

    [Fact]
    public void MakesCommandCancellationWithParam()
    {
      var cmd = Guid.NewGuid().ToString();
      var dut = PipeMessageFactory.Instance;
      var param = new Exception();

      var result = dut.CreateCommandCancellation(cmd, param);

      result.Command.Should().Be(cmd);
      result.PacketType.Should().Be(PacketType.CommandCancellation);
      result.Parameter.Should().Be(param);

      param = new Exception();

      var result2 = dut.CreateCommandCancellation(cmd, param);

      result2.Command.Should().Be(cmd);
      result2.PacketType.Should().Be(PacketType.CommandCancellation);
      result2.Parameter.Should().Be(param);

      result.Id.Should().NotBe(result2.Id);
    }

    [Fact]
    public void MakesCommandFailureWithParam()
    {
      var cmd = Guid.NewGuid().ToString();
      var dut = PipeMessageFactory.Instance;
      var param = new Exception();

      var result = dut.CreateCommandFailure(cmd, param);

      result.Command.Should().Be(cmd);
      result.PacketType.Should().Be(PacketType.CommandFailure);
      result.Parameter.Should().Be(param);

      param = new Exception();

      var result2 = dut.CreateCommandFailure(cmd, param);

      result2.Command.Should().Be(cmd);
      result2.PacketType.Should().Be(PacketType.CommandFailure);
      result2.Parameter.Should().Be(param);

      result.Id.Should().NotBe(result2.Id);
    }

    [Fact]
    public void MakesResponse()
    {
      var cmd = Guid.NewGuid().ToString();
      var dut = PipeMessageFactory.Instance;

      var command = dut.CreateCommand(cmd);
      var result = dut.CreateResponse(command);

      result.Id.Should().Be(command.Id);
      result.Command.Should().Be(command.Command);
      result.PacketType.Should().Be(PacketType.Response);
      result.Parameter.Should().BeNull();

      var command2 = dut.CreateCommand(cmd);
      var result2 = dut.CreateResponse(command2);

      result2.Id.Should().Be(command2.Id);
      result2.Command.Should().Be(command2.Command);
      result2.PacketType.Should().Be(PacketType.Response);
      result2.Parameter.Should().BeNull();

      result.Id.Should().NotBe(result2.Id);
    }

    [Fact]
    public void MakesResponseWithParameter()
    {
      var cmd = Guid.NewGuid().ToString();
      var param = Guid.NewGuid();
      var dut = PipeMessageFactory.Instance;

      var command = dut.CreateCommand(cmd);
      var result = dut.CreateResponse(command, param);

      result.Id.Should().Be(command.Id);
      result.Command.Should().Be(command.Command);
      result.PacketType.Should().Be(PacketType.Response);
      result.Parameter.Should().Be(param);

      var command2 = dut.CreateCommand(cmd);
      var result2 = dut.CreateResponse(command2, param);

      result2.Id.Should().Be(command2.Id);
      result2.Command.Should().Be(command2.Command);
      result2.PacketType.Should().Be(PacketType.Response);
      result2.Parameter.Should().Be(param);

      result.Id.Should().NotBe(result2.Id);
    }

    [Fact]
    public void MakesResponseCancellation()
    {
      var cmd = Guid.NewGuid().ToString();
      var dut = PipeMessageFactory.Instance;

      var command = dut.CreateCommand(cmd);
      var result = dut.CreateResponseCancellation(command);

      result.Id.Should().Be(command.Id);
      result.Command.Should().Be(command.Command);
      result.PacketType.Should().Be(PacketType.ResponseCancellation);
      result.Parameter.Should().BeNull();

      var command2 = dut.CreateCommand(cmd);
      var result2 = dut.CreateResponseCancellation(command2);

      result2.Id.Should().Be(command2.Id);
      result2.Command.Should().Be(command2.Command);
      result2.PacketType.Should().Be(PacketType.ResponseCancellation);
      result2.Parameter.Should().BeNull();

      result.Id.Should().NotBe(result2.Id);
    }

    [Fact]
    public void MakesResponseCancellationWithParam()
    {
      var cmd = Guid.NewGuid().ToString();
      var param = new Exception();
      var dut = PipeMessageFactory.Instance;

      var command = dut.CreateCommand(cmd);
      var result = dut.CreateResponseCancellation(command, param);

      result.Id.Should().Be(command.Id);
      result.Command.Should().Be(command.Command);
      result.PacketType.Should().Be(PacketType.ResponseCancellation);
      result.Parameter.Should().Be(param);

      var command2 = dut.CreateCommand(cmd);
      var result2 = dut.CreateResponseCancellation(command2, param);

      result2.Id.Should().Be(command2.Id);
      result2.Command.Should().Be(command2.Command);
      result2.PacketType.Should().Be(PacketType.ResponseCancellation);
      result2.Parameter.Should().Be(param);

      result.Id.Should().NotBe(result2.Id);
    }

    [Fact]
    public void MakesResponseFailureWithParam()
    {
      var cmd = Guid.NewGuid().ToString();
      var param = new Exception();
      var dut = PipeMessageFactory.Instance;

      var command = dut.CreateCommand(cmd);
      var result = dut.CreateResponseFailure(command, param);

      result.Id.Should().Be(command.Id);
      result.Command.Should().Be(command.Command);
      result.PacketType.Should().Be(PacketType.ResponseFailure);
      result.Parameter.Should().Be(param);

      var command2 = dut.CreateCommand(cmd);
      var result2 = dut.CreateResponseFailure(command2, param);

      result2.Id.Should().Be(command2.Id);
      result2.Command.Should().Be(command2.Command);
      result2.PacketType.Should().Be(PacketType.ResponseFailure);
      result2.Parameter.Should().Be(param);

      result.Id.Should().NotBe(result2.Id);
    }
  }
}
