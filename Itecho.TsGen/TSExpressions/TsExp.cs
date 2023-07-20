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

    public static ImportExp Import(string library, ImportExp.NamedImport @default, params ImportExp.NamedImport[] imports) =>
        new(library, @default, imports);

    public static DefaultExportExp DefaultExport(TsExpBase exp) => new(exp);
    public static NamedExportExp NamedExport(TsStandaloneExp exp) => new(exp);
    public static InterfaceExp Interface(TsInterface @interface) => new(@interface);

    public static TsEmptyLineExp EmptyLine() => new();
    public static DictionaryExp Dictionary(Dictionary<TsExp, TsExp> values) => new(values);
    public static FunctionExp Function(string name, TsType returnType, IEnumerable<TsParameter> parameters, TsBlockExp block) => new(name, returnType, parameters, block);

    public static ConstExp String(string value) => new(value);
    public static ConstExp Number(int value) => new(value);
    public static ConstExp Number(double value) => new(value);
    public static ConstExp Boolean(bool value) => new(value);
    public static ReturnExp Return(TsExp expression) => new(expression);
    public static TsBlockExp Block(params TsStandaloneExp[] lines) => new(lines);
    public static TsParameter Parameter(string name, TsType type) => new(name, type);
    public static AssignExp
}
