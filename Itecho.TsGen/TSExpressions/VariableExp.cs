namespace Itecho.TsGen.TSExpressions;

public class VariableExp : TsExp
{
    public string Name { get; }

    public VariableExp(string name)
    {
        Name = name;
    }
    public override void Write(TsCodeGenerator gen)
    {
        gen.Write(Name);
    }
}
