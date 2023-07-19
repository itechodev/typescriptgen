namespace Itecho.TsGen.TSExpressions;

public class NamedExportExp : TsStandaloneExp
{
    public TsStandaloneExp Export { get; }

    public NamedExportExp(TsStandaloneExp export)
    {
        Export = export;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write("export ");
        Export.Write(gen);
    }
}