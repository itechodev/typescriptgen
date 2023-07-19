using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class TsFile
{
    // ES6 imports must be found at the top level of your module
    public List<ImportExp> Imports { get; } = new();

    // Ts file only allowed to have TsRootExp
    public List<TsRootExp> Body { get; } = new();

    public void AddImport(ImportExp import)
    {
        Imports.Add(import);
    }

    public void AddBody(TsRootExp exp)
    {
        Body.Add(exp);
    }

    public void WriteToFile(string fileName)
    {
        var gen = new TsCodeGenerator();
        foreach (var import in Imports)
        {
            import.Write(gen);
        }

        foreach (var exp in Body)
        {
            exp.Write(gen);
        }

        gen.WriteToFile(fileName + ".ts");
    }
}

/// <summary>
/// Top level expression which cannot be nested
/// such as comments, import, export, interface
/// most top level expression does contain other expressions
/// </summary>
public abstract class TsRootExp
{
    public abstract void Write(TsCodeGenerator gen);
}

public class CommentExp : TsRootExp
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
            gen.WriteLine($"/** {Comment} */");
        else
            gen.WriteLine($"// {Comment}");
    }
}

/// <summary>
/// ES6 imports must be found at the top level of your module
/// </summary>
public class ImportExp
{
    public string Library { get; }
    public string? Default { get; }
    public NamedImport[] Imports { get; }

    public class NamedImport
    {
        // import {name} from ..
        public string Name { get; }

        // import {type name} from
        public bool IsTyped { get; }

        public NamedImport(string name, bool isTyped)
        {
            Name = name;
            IsTyped = isTyped;
        }

        public string Generate()
        {
            var str = IsTyped
                ? "type " + Name
                : Name;

            return $"{str}";
        }
    }

    public ImportExp(string library, string? @default, params NamedImport[] imports)
    {
        Library = library;
        Default = @default;
        Imports = imports;
    }

    public void Write(TsCodeGenerator gen)
    {
        var strings = Imports.Select(i => i.Generate());
        if (@Default != null)
            strings = strings.Prepend(@Default);

        gen.WriteLine($"import {{ {string.Join(", ", strings)} }} from '{Library}';");
    }
}

public class ExportExp : TsRootExp
{
    public override void Write(TsCodeGenerator gen)
    {
    }
}

public class TsParameter
{
    public string Name { get; }
    public TsType Type { get; }

    public TsParameter(string name, TsType type)
    {
        Name = name;
        Type = type;
    }
}

public class TsBlockExp : TsRootExp
{
    public TsRootExp[] Expressions { get; }

    public TsBlockExp(TsRootExp[] expressions)
    {
        Expressions = expressions;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}

public class FunctionExp : TsRootExp
{
    public string Name { get; }
    public TsType ReturnType { get; }
    public TsParameter[] Parameters { get; }
    public TsBlockExp Block { get; }

    public FunctionExp(string name, TsType returnType, TsParameter[] parameters, TsBlockExp block)
    {
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
        Block = block;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}

public enum VariableType
{
    Let,
    Const,
    Var
}

public class VariableAssignExp : TsRootExp
{
    public string Name { get; }
    public TsExp Value { get; }
    public VariableType Type { get; }

    // null for variable inference
    public TsType? Signature { get; }

    public VariableAssignExp(string name, TsExp value, VariableType type, TsType? signature = null)
    {
        Name = name;
        Value = value;
        Type = type;
        Signature = signature;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}

public class InterfaceExp : TsRootExp
{
    public TsInterface @Interface { get; }

    public InterfaceExp(TsInterface @interface)
    {
        Interface = @interface;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}

public class DictionaryExp : TsExp
{
    public Dictionary<TsExp, TsExp> Values { get; }

    public DictionaryExp(Dictionary<TsExp, TsExp> values)
    {
        Values = values;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}

public class ReturnExp : TsRootExp
{
    public TsExp Return { get; }

    public ReturnExp(TsExp @return)
    {
        Return = @return;
    }

    public override void Write(TsCodeGenerator gen)
    {
        throw new NotImplementedException();
    }
}

public class MultilineCommentExp : TsRootExp
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

/// <summary>
/// Expression that is nestable inside other expressions
/// such as 
/// </summary>
public abstract class TsExp : TsRootExp
{
    // factory methods to create expression
    public static CommentExp Comment(string comment, bool isJsDoc = false) => new(comment, isJsDoc);
    public static MultilineCommentExp MultilineComment(params string[] lines) => new(lines);

    public static ImportExp Import(string library, string? @default, params ImportExp.NamedImport[] imports) =>
        new(library, @default, imports);
}

public class ObjectAccessExp : TsExp
{
    public TsExp Object { get; }
    public string Access { get; }

    public ObjectAccessExp(TsExp o, string access)
    {
        Object = o;
        Access = access;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}

public class ConstExp : TsExp
{
    public object? Value { get; }
    public TsPrimitive.TsPrimitiveType Type { get; }

    public ConstExp(object? value, TsPrimitive.TsPrimitiveType type)
    {
        Value = value;
        Type = type;
    }

    public ConstExp(string? value) : this(value, TsPrimitive.TsPrimitiveType.String)
    {
    }

    public ConstExp(double value) : this(value, TsPrimitive.TsPrimitiveType.Number)
    {
    }

    public ConstExp(int value) : this(value, TsPrimitive.TsPrimitiveType.Number)
    {
    }

    public ConstExp(bool value) : this(value, TsPrimitive.TsPrimitiveType.Boolean)
    {
    }

    public ConstExp(DateTime value) : this(value, TsPrimitive.TsPrimitiveType.Date)
    {
    }

    public override void Write(TsCodeGenerator gen)
    {
        throw new NotImplementedException();
    }
}