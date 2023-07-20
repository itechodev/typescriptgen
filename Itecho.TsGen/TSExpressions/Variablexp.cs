using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class Variablexp : TsExp
{
    public string Name;
    public VariableType Type { get; }
    // null for variable inference
    public TsType? Signature { get; }

    public override void Write(TsCodeGenerator gen)
    {
        throw new NotImplementedException();
    }
    
}
