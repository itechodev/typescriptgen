namespace Itecho.TsGen.TSExpressions;

public class LiteralExp: TsExp {
    
    public string Text { get; }

    public LiteralExp(string text)
    {
        Text = text;
    }
    public override void Write(TsCodeGenerator gen)
    {
        gen.Write(Text);
    }
}
