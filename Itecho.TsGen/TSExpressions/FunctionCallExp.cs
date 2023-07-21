namespace Itecho.TsGen.TSExpressions;

public class FunctionCallExp : TsExp
{
    public TsExp Exp { get; }
    public IEnumerable<TsExp> Parameters { get; }

    public FunctionCallExp(TsExp exp, IEnumerable<TsExp> parameters)
    {
        Exp = exp;
        Parameters = parameters;
    }

    public override void Write(TsCodeGenerator gen)
    {
        Exp.Write(gen);
        gen.Write("(");
        foreach (var param in Parameters)
        {
            param.Write(gen);
        }
        gen.Write(")");
    }
}
