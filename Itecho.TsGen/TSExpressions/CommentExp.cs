namespace Itecho.TsGen.TSExpressions;

public class CommentExp : TsStandaloneExp
{
    private bool IsJsDoc { get; }
    private string Comment { get; }

    public CommentExp(string comment, bool isJsDoc = false)
    {
        IsJsDoc = isJsDoc;
        Comment = comment;
    }

    public override void Write(TsCodeGenerator gen)
    {
        if (IsJsDoc)
            gen.WriteLine($"/* {Comment} */");
        else
            gen.WriteLine($"// {Comment}");
    }
}