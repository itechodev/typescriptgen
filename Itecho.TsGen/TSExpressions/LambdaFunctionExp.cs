using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class LambdaFunctionExp : TsExp
{
    public TsType? ReturnType { get; }

    public IEnumerable<TsParameter> Parameters { get; }

    // block or implicit return
    public new TsExp Block { get; }

    public LambdaFunctionExp(TsType? returnType, IEnumerable<TsParameter> parameters, TsExp block)
    {
        ReturnType = returnType;
        Parameters = parameters;
        Block = block;
    }

    public override void Write(TsCodeGenerator gen)
    {
        var genericResolver = new TsGenericTypeResolver();
        if (ReturnType != null)
            genericResolver.Visit(ReturnType);
        foreach (var param in Parameters)
            genericResolver.Visit(param.Type);

        if (genericResolver.List.Any())
        {
            gen.Write("<");
            gen.Write(string.Join(", ", genericResolver.List));
            gen.Write(">");   
        }

        // (p: number)?: returnType => "";
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
        if (ReturnType != null)
        {
            gen.Write(": ");
            gen.Write(TsTypeGenerator.Generate(ReturnType));
        }

        gen.Write(" => ");
        // implicit return 
        if (Block is ReturnExp ret)
        {
            ret.Expression.Write(gen);
        }
        else Block.Write(gen);
    }
}