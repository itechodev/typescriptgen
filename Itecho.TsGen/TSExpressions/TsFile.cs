namespace Itecho.TsGen.TSExpressions;

public class TsFile
{
    // ES6 imports must be found at the top level of your module
    public List<ImportExp> Imports { get; } = new();

    // Ts file only allowed to have TsRootExp
    public List<TsStandaloneExp> Body { get; } = new();

    public void AddImport(ImportExp import)
    {
        Imports.Add(import);
    }

    public void AddBody(TsStandaloneExp exp)
    {
        Body.Add(exp);
    }

    public void WriteToFile(string fileName)
    {
        var gen = new TsCodeGenerator();
        foreach (var import in Imports)
        {
            import.Write(gen);
        }

        foreach (var exp in Body)
        {
            exp.Write(gen);
        }

        gen.WriteToFile(fileName + ".ts");
    }
}