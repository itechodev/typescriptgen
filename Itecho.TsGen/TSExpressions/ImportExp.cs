namespace Itecho.TsGen.TSExpressions;

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