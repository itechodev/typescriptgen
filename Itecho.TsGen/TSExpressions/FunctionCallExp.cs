namespace Itecho.TsGen.TSExpressions;

public class FunctionCallExp : TsExp
{
    public string Name { get; }
    public IEnumerable<TsExp> Parameters { get; }

    public FunctionCallExp(string name, IEnumerable<TsExp> parameters)
    {
        Name = name;
        Parameters = parameters;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write(Name);
        gen.Write("(");
        foreach (var param in Parameters)
        {
            param.Write(gen);
        }
        gen.Write(")");
    }
}
