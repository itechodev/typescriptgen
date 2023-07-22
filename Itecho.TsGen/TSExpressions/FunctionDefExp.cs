using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class FunctionDefExp : TsExp
{
    public string Name { get; }
    public TsType ReturnType { get; }
    public IEnumerable<TsParameter> Parameters { get; }
    public new TsBlockExp Block { get; }

    public FunctionDefExp(string name, TsType returnType, IEnumerable<TsParameter> parameters, TsBlockExp block)
    {
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
        Block = block;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write("function ");
        gen.Write(Name);
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
        gen.Write(":");
        gen.Write(TsTypeGenerator.Generate(ReturnType));
        Block.Write(gen);
    }
}