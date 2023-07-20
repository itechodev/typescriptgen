namespace Itecho.TsGen.TSExpressions;

public class StringExp : TsExp
{
    private string Value { get; }
    public StringExp(string value)
    {
        Value = value;
    }
    
    public override void Write(TsCodeGenerator gen)
    {
        gen.Write("'" + Value + "'");
    }
}


