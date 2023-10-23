namespace Itecho.TsGen.TSExpressions;

/// <summary>
/// ES6 imports must be found at the top level of your module
/// </summary>
public class ImportExp : TsExp
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
            return (IsTyped ? "type " : "") + Name;
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
        // type-only import can specify a default import or named bindings, but not both.
        if (@Default?.IsTyped ?? false)
        {
            gen.Write(FormatHelper.Spaces(
                "import",
                @Default?.Generate(),
                $"from \"{Library}\";"
            ));
            gen.NewLine();
            
            if (Imports.Any())
            {
                gen.Write(FormatHelper.Spaces(
                    "import",
                    Imports.Any() ? "{" + string.Join(", ", Imports.Select(i => i.Generate())) + "}" : string.Empty,
                    $"from \"{Library}\";"
                ));
                gen.NewLine();
            }
        }
        else
        {
            gen.Write(FormatHelper.Spaces(
                "import",
                @Default?.Generate(),
                Imports.Any() ? ", {" + string.Join(", ", Imports.Select(i => i.Generate())) + "}" : string.Empty,
                $"from \"{Library}\";"
            ));
            gen.NewLine();
        }
        
    }
}
