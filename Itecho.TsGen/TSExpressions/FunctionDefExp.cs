using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class FunctionDefExp : TsExp
{
    public string Name { get; }
    public TsType ReturnType { get; }
    public IEnumerable<TsParameter> Parameters { get; }
    public TsBlockExp Block { get; }

    public FunctionDefExp(string name, TsType returnType, IEnumerable<TsParameter> parameters, TsBlockExp block)
    {
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
        Block = block;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write("function");
        gen.Write(Name);
        gen.Write("(");
        foreach (var param in Parameters)
        {
            param.Write(gen);
        }
        gen.Write(")");
        gen.Write(":");
        TsTypeGenerator.Generate(ReturnType);
        Block.Write(gen);
        // function aa(p) {}
    }
}