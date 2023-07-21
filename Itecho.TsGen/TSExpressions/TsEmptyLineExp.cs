namespace Itecho.TsGen.TSExpressions;

public class TsEmptyLineExp : TsExp
{
    public override void Write(TsCodeGenerator gen)
    {
        gen.NewLine();
    }
}