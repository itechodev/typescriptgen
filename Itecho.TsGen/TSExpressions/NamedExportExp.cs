namespace Itecho.TsGen.TSExpressions;

public class NamedExportExp : TsExp
{
    public TsExp Export { get; }

    public NamedExportExp(TsExp export)
    {
        Export = export;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write("export ");
        Export.Write(gen);
    }
}