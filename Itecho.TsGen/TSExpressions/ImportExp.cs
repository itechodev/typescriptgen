namespace Itecho.TsGen.TSExpressions;

/// <summary>
/// ES6 imports must be found at the top level of your module
/// </summary>
public class ImportExp : TsStandaloneExp
{
    public string Library { get; }
    public NamedImport? Default { get; }
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

    public ImportExp(string library, NamedImport? @default, params NamedImport[] imports)
    {
        Library = library;
        Default = @default;
        Imports = imports;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.WriteLine(FormatHelper.Spaces(
            "import",
            @Default?.Generate(),
            string.Join(", ", Imports.Select(i => i.Generate())),
            $"from \"{Library}\";"
        ));
    }
}