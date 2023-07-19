namespace Itecho.TsGen.TSExpressions;

public class TsBlockExp : TsStandaloneExp
{
    public TsStandaloneExp[] Expressions { get; }

    public TsBlockExp(TsStandaloneExp[] expressions)
    {
        Expressions = expressions;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}