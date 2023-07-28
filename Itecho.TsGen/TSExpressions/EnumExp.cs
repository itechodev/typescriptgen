using System.Runtime.CompilerServices;
using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class EnumExp : TsExp
{
    public new TsEnum @Enum { get;  }

    public EnumExp(TsEnum @enum)
    {
        Enum = @enum;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write($"enum {@Enum.Name}");
        gen.Block(() => {
            if (!@Enum.Values.Any()) return;
            WriteItem(gen, @Enum.Values.First(), false);
            foreach (var key in @Enum.Values.Skip(1))
            {
                WriteItem(gen, key, true);
            }
        }, true);
    }
    private void WriteItem(TsCodeGenerator gen, KeyValuePair<string, int> key, bool comma)
    {
        if (comma)
        {
            gen.Write(",");
            gen.NewLine();
        }
        gen.Write(key.Key);
        gen.Write("=", true);
        if (@Enum.ValueType is TsEnum.TsEnumValueType.Number or TsEnum.TsEnumValueType.Undefined)
        {
            gen.Write(key.Value.ToString(), true);
        }
        else
        {
            gen.Write("\"" + key.Key + "\"", true);
        }
    }
}
