using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class InterfaceExp : TsStandaloneExp
{
    public TsInterface @Interface { get; }

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

        if (@Interface.Extends != null)
        {
            gen.Write($" extends {@Interface.Extends.Name}");
        }

        gen.Block(() =>
        {
            foreach (var member in @Interface.Members)
            {
                gen.WriteLine($"{member.Name}: {TsTypeGenerator.Generate(member.Type)};");
            }
        });
    }
}