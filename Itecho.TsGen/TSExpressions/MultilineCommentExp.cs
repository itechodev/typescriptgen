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
        gen.WriteLine("/**");
        foreach (var line in Lines)
        {
            gen.WriteLine($" * {line}");
        }

        gen.WriteLine(" */");
    }
}