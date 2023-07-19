namespace Itecho.TsGen.TSExpressions;

public class DefaultExportExp : TsStandaloneExp
{
    public TsExpBase Expression { get; }

    public DefaultExportExp(TsExpBase expression)
    {
        Expression = expression;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write("export default ");
        Expression.Write(gen);
    }
}