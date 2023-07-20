namespace Itecho.TsGen.TSExpressions;

public class ReturnExp : TsStandaloneExp
{
    public TsExp Expression { get; }
    public ReturnExp(TsExp expression)
    {
        Expression = expression;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.WriteLine($"return ");
        Expression.Write(gen);
    }
}
