namespace Itecho.TsGen.TSExpressions;

public class MultilineCommentExp : TsExp
{
    public string[] Lines { get; }

    public MultilineCommentExp(params string[] lines)
    {
        Lines = lines;
    }


    public override void Write(TsCodeGenerator gen)
    {
        gen.Write("/**");
        foreach (var line in Lines)
        {
            gen.NewLine();
            gen.Write($" * {line}");
        }

        gen.NewLine();
        gen.Write(" */");
        gen.NewLine();
    }
}