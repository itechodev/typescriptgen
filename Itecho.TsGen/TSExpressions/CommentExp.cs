namespace Itecho.TsGen.TSExpressions;

public class CommentExp : TsExp
{
    private bool IsJsDoc { get; }
    private new string Comment { get; }

    public CommentExp(string comment, bool isJsDoc = false)
    {
        IsJsDoc = isJsDoc;
        Comment = comment;
    }

    public override void Write(TsCodeGenerator gen)
    {
        if (IsJsDoc)
            gen.Write($"/* {Comment} */");
        else
            gen.Write($"// {Comment}");
        
        gen.NewLine();
    }
}