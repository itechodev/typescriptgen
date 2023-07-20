using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

/// <summary>
/// Expression that is nestable inside other expressions
/// such as 
/// </summary>
public abstract class TsExp
{
    public abstract void Write(TsCodeGenerator gen);


    // factory methods to create expression
    public static CommentExp Comment(string comment, bool isJsDoc = false) => new(comment, isJsDoc);
    public static MultilineCommentExp MultilineComment(params string[] lines) => new(lines);

    public static ImportExp Import(string library, ImportExp.NamedImport @default, params ImportExp.NamedImport[] imports) =>
        new(library, @default, imports);

    public static DefaultExportExp DefaultExport(TsExp exp) => new(exp);
    public static NamedExportExp NamedExport(TsExp exp) => new(exp);
    public static InterfaceExp Interface(TsInterface @interface) => new(@interface);

    public static TsEmptyLineExp EmptyLine() => new();
    public static DictionaryExp Dictionary(IEnumerable<DictionaryEntry> entries) => new(entries);
    public static FunctionExp Function(string name, TsType returnType, IEnumerable<TsParameter> parameters, TsBlockExp block) => new(name, returnType, parameters, block);
    public static LambdaFunctionExp Lambda(TsType? returnType, IEnumerable<TsParameter> parameters, TsExp block) => new(returnType, parameters, block);

    public static StringExp String(string value) => new(value);
    public static ReturnExp Return(TsExp expression) => new(expression);
    public static TsBlockExp Block(params TsExp[] lines) => new(lines);
    public static TsParameter Parameter(string name, TsType type) => new(name, type);
    public static VariableExp Variable(string name, VariableType type, TsType? signature) => new(name, type, signature);
    public static AssignExp Assign(TsExp name, TsExp value) => new(name, value);
}
