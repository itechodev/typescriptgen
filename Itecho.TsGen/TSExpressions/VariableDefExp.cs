using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class VariableDefExp : TsExp
{
    public string Name;
    public VariableType Type { get; }
    // null for variable inference
    public TsType? Signature { get; }

    public VariableDefExp(string name, VariableType type, TsType? signature)
    {
        Name = name;
        Type = type;
        Signature = signature;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write(VariableTypeString(Type));
        gen.Write(Name, true);
        if (Signature != null)
        {
            gen.Write(":");
            gen.Write(TsTypeGenerator.Generate(Signature), true);
        }
    }

    private static string VariableTypeString(VariableType type)
    {
        return type switch
        {
            VariableType.Const => "const", VariableType.Let => "let", VariableType.Var => "var", _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

}
