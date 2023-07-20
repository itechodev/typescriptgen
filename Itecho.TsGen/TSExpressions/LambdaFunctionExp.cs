using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class LambdaFunctionExp : TsExp
{
    public TsType? ReturnType { get; }
    public IEnumerable<TsParameter> Parameters { get; }
    // block or implicit return
    public TsExp Block { get; }

    public LambdaFunctionExp(TsType? returnType, IEnumerable<TsParameter> parameters, TsExp block)
    {
        ReturnType = returnType;
        Parameters = parameters;
        Block = block;
    }

    public override void Write(TsCodeGenerator gen)
    {
        // (p: number)?: returnType => "";
        gen.Write("(");
        foreach (var param in Parameters)
        {
            param.Write(gen);
        }
        gen.Write(")");
        if (ReturnType != null)
        {
            gen.Write(":");
            gen.Write(TsTypeGenerator.Generate(ReturnType));
        }
        gen.Write("=>");
        Block.Write(gen);
    }
}
