namespace Itecho.TsGen.TSExpressions;

public class AssignExp : TsExp
{
    public new TsExp Assign { get; }
    public TsExp Value { get; }

    public AssignExp(TsExp name, TsExp value)
    {
        Assign = name;
        Value = value;
    }

    public override void Write(TsCodeGenerator gen)
    {
        Assign.Write(gen);
        gen.Write(" = ");
        Value.Write(gen);
        gen.NewLine();
    }
}
