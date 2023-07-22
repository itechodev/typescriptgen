using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class FunctionCallExp : TsExp
{
    public TsExp Exp { get; }
    public IEnumerable<TsType>? Generics { get; }
    public IEnumerable<TsExp> Parameters { get; }

    public FunctionCallExp(TsExp exp, IEnumerable<TsType>? generics, IEnumerable<TsExp> parameters)
    {
        Generics = generics;
        Exp = exp;
        Parameters = parameters;
    }

    public override void Write(TsCodeGenerator gen)
    {
        Exp.Write(gen);
        if (Generics != null && Generics.Any())
        {
            gen.Write("<");
            gen.Write(string.Join(", ", Generics.Select(TsTypeGenerator.Generate)));
            gen.Write(">");
        }
        gen.Write("(");
        if (Parameters.Any())
        {
            Parameters.First().Write(gen);
            foreach (var param in Parameters.Skip(1))
            {
                gen.Write(", ");
                param.Write(gen);
            }
        }

        gen.Write(")");
    }
}
