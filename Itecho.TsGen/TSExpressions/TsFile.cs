namespace Itecho.TsGen.TSExpressions;

public class TsFile
{
    // Ts file only allowed to have TsRootExp
    public List<TsExp> Body { get; } = new();

    public void Add(TsExp exp)
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

        var tsFileName = fileName.EndsWith(".ts") 
            ? fileName 
            : fileName + ".ts";
        
        gen.WriteToFile(tsFileName);
    }
}
