using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class AssignExp : TsExp
{
    public TsExp Assign { get; }
    public TsExp Value { get; }

    public AssignExp(TsExp name, TsExp value, VariableType type, TsType? signature = null)
    {
        Assign = name;
        Value = value;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}
