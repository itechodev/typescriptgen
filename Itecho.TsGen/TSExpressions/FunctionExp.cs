using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class FunctionExp : TsStandaloneExp
{
    public string Name { get; }
    public TsType ReturnType { get; }
    public TsParameter[] Parameters { get; }
    public TsBlockExp Block { get; }

    public FunctionExp(string name, TsType returnType, TsParameter[] parameters, TsBlockExp block)
    {
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
        Block = block;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}