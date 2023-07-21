namespace Itecho.TsGen.TSExpressions;

public class TsBlockExp : TsExp
{
    public TsExp[] Expressions { get; }

    public TsBlockExp(TsExp[] expressions)
    {
        Expressions = expressions;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Block(() =>
        {
            foreach (var exp in Expressions)
            {
                exp.Write(gen);
            }
        });
    }
}