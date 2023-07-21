namespace Itecho.TsGen.TSExpressions;

public class ReturnExp : TsExp
{
    public TsExp Expression { get; }
    public ReturnExp(TsExp expression)
    {
        Expression = expression;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.NewLine();
        gen.Write($"return ");
        Expression.Write(gen);
    }
}
