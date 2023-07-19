namespace Itecho.TsGen.TSExpressions;

public class TsEmptyLineExp : TsStandaloneExp
{
    public override void Write(TsCodeGenerator gen)
    {
        gen.WriteLine();
    }
}