using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class VariableExp : TsExp
{
    public string Name;
    public VariableType Type { get; }
    // null for variable inference
    public TsType? Signature { get; }

    public VariableExp(string name, VariableType type, TsType? signature)
    {
        Name = name;
        Type = type;
        Signature = signature;
    }

    public override void Write(TsCodeGenerator gen)
    {
        throw new NotImplementedException();
    }
    
}
