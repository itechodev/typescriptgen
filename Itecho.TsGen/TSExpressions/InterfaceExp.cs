using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class InterfaceExp : TsExp
{
    public new TsInterface @Interface { get; }

    public InterfaceExp(TsInterface @interface)
    {
        Interface = @interface;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write($"interface {@Interface.Name}");
        if (@Interface.Generics.Length > 0)
        {
            gen.Write($"<{string.Join(", ", Interface.Generics.Select(g => g.Name))}>");
        }

        if (@Interface.Extends.Any())
        {
            gen.Write($"extends {string.Join(", ",@Interface.Extends.Select(e => e.Name))}", true);
        }

        gen.Block(() => {
            foreach (var member in @Interface.Members)
            {
                gen.Write($"{FormatHelper.CamelCase(member.Name)}: {TsTypeGenerator.Generate(member.Type)};");
                gen.NewLine();
            }
        }, true);
    }
}
