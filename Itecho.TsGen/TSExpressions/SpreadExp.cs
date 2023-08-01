namespace Itecho.TsGen.TSExpressions;

public class SpreadExp : TsExp
{
    public TsExp Exp { get; set; }

    public SpreadExp(TsExp exp)
    {
        Exp = exp;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write("...");
        Exp.Write(gen);
    }
}
