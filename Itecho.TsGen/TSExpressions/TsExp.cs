using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

/// <summary>
/// Expression that is nestable inside other expressions
/// such as 
/// </summary>
public abstract class TsExp : TsExpBase
{

    // factory methods to create expression
    public static CommentExp Comment(string comment, bool isJsDoc = false) => new(comment, isJsDoc);
    public static MultilineCommentExp MultilineComment(params string[] lines) => new(lines);

    public static ImportExp Import(string library, string? @default, params ImportExp.NamedImport[] imports) =>
        new(library, @default, imports);

    public static DefaultExportExp DefaultExport(TsExpBase exp) => new(exp);
    public static NamedExportExp NamedExport(TsStandaloneExp exp) => new(exp);
    public static InterfaceExp Interface(TsInterface @interface) => new(@interface);

    public static TsEmptyLineExp EmptyLine() => new();
}