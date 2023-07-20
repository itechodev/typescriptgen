namespace Itecho.TsGen.TSExpressions;

public class DefaultExportExp : TsExp
{
    public TsExp Expression { get; }

    public DefaultExportExp(TsExp expression)
    {
        Expression = expression;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write("export default ");
        Expression.Write(gen);
    }
}
