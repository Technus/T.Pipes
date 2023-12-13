namespace T.Pipes.Test
{
  [ExcludeFromCodeCoverage]
  public class FormatterTests
  {
    [Fact]
    public void SerializeByte()
    {
      var dut = new Formatter();

      byte a = 3;

      var s = dut.Serialize(new PipeMessage() { Parameter = a });

      var o = dut.Deserialize<object>(s);

      o.Should().BeOfType<PipeMessage>();
      ((PipeMessage)o!).Parameter.Should().BeOfType<byte>();
    }

    [Fact]
    public void SerializeFloat()
    {
      var dut = new Formatter();

      float a = 3.5f;

      var s = dut.Serialize(new PipeMessage() { Parameter = a });

      var o = dut.Deserialize<object>(s);

      o.Should().BeOfType<PipeMessage>();
      ((PipeMessage)o!).Parameter.Should().BeOfType<float>();
    }

    [Fact]
    public void SerializeFloatByte()
    {
      var dut = new Formatter();

      (float,byte) a = (3.5f,8);

      var s = dut.Serialize(new PipeMessage() { Parameter = a });

      var o = dut.Deserialize<object>(s);

      o.Should().BeOfType<PipeMessage>();
      ((PipeMessage)o!).Parameter.Should().BeOfType<(float, byte)>();
    }
  }
}
