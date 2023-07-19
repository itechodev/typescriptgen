using System.Text;
using System.Text.Json;

namespace Itecho.TsGen;

public class TsCodeGenerator
{
    private int _indent;

    private readonly StringBuilder _builder = new();

    public void Indent()
    {
        _indent++;
    }

    public void Outdent()
    {
        _indent--;
    }

    public void Write(string text)
    {
    }

    public void WriteLine(params string[] lines)
    {
        _builder.Append(string.Join("," + Environment.NewLine, lines.Select(l => new string(' ', _indent * 4) + l)));
        _builder.AppendLine();
    }

    // wrap writings inside a block with indenting
    public void Block(Action<TsCodeGenerator> action)
    {
        Write("{");
        WriteLine();
        Indent();
        action.Invoke(this);
        Outdent();
        WriteLine("}");
    }

    public static string CamelCase(string s)
    {
        return JsonNamingPolicy.CamelCase.ConvertName(s);
    }

    public void Function(string name, params string[] args)
    {
        WriteLine($"{name}({string.Join(", ", args)})");
    }

    public void WriteToFile(string fileName)
    {
        File.WriteAllText(fileName, _builder.ToString());
    }
}