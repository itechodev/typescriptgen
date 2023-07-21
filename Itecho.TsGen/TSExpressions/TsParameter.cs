using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class TsParameter
{
    public string Name { get; }
    public TsType Type { get; }

    public TsParameter(string name, TsType type)
    {
        Name = name;
        Type = type;
    }
    public void Write(TsCodeGenerator gen)
    {
        gen.Write(Name);
        gen.Write(":");
        gen.Write(TsTypeGenerator.Generate(Type));
    }
}
