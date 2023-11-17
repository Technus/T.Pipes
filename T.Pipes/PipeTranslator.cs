using T.Pipes.Abstractions;

namespace T.Pipes
{
  public partial class PipeTranslator
  {

    public partial class A<Tea>
    {
      [PipeMe]
      partial void Kek();

    }

  }
}
