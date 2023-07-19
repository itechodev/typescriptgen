using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class VariableAssignExp : TsStandaloneExp
{
    public string Name { get; }
    public TsExp Value { get; }
    public VariableType Type { get; }

    // null for variable inference
    public TsType? Signature { get; }

    public VariableAssignExp(string name, TsExp value, VariableType type, TsType? signature = null)
    {
        Name = name;
        Value = value;
        Type = type;
        Signature = signature;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}