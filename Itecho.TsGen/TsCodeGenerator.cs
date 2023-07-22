using System.Text;
using System.Text.Json;
using Itecho.TsGen.TSExpressions;

namespace Itecho.TsGen;

public class TsCodeGenerator
{
    private int _indent;

    private readonly StringBuilder _builder = new();

    private bool _startOfNewLine = false;

    public void Indent()
    {
        _indent++;
    }

    public void Outdent()
    {
        _indent--;
    }

    public void Write(string text, bool spaceBefore = false)
    {
        if (_startOfNewLine)
        {
            _builder.Append(new string(' ', _indent * 4) + text);
            _startOfNewLine = false;
            return;
        }

        _builder.Append((spaceBefore ? " " : "") + text);
    }

    // wrap writings inside a block with indenting
    public void Block(Action action, bool spaceBefore = false)
    {
        Write("{", spaceBefore);
        Indent();
        NewLine();
        action.Invoke();
        Outdent();
        if (!_startOfNewLine)
            NewLine();
        Write("}");
    }


    public void NewLine()
    {
        _builder.Append("\n");
        _startOfNewLine = true;
    }

    public void WriteToFile(string fileName)
    {
        File.WriteAllText(fileName, _builder.ToString());
    }

}
