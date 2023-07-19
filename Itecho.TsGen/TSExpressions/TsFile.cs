namespace Itecho.TsGen.TSExpressions;

public class TsFile
{
    // Ts file only allowed to have TsRootExp
    public List<TsStandaloneExp> Body { get; } = new();

    public void Add(TsStandaloneExp exp)
    {
        Body.Add(exp);
    }

    public void WriteToFile(string fileName)
    {
        var gen = new TsCodeGenerator();
        foreach (var exp in Body)
        {
            exp.Write(gen);
        }

        gen.WriteToFile(fileName + ".ts");
    }
}