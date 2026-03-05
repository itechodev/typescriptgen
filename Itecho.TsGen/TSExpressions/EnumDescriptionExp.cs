using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class EnumDescriptionExp : TsExp
{
    public new TsEnum Enum { get; }

    public EnumDescriptionExp(TsEnum @enum)
    {
        Enum = @enum;
    }

    public override void Write(TsCodeGenerator gen)
    {
        if (!Enum.HasDescriptions) return;

        // Generate: export const StatusDescription: Record<Status, string> = {
        gen.Write($"const {Enum.Name}Description: Record<{Enum.Name}, string> =");
        gen.Block(() =>
        {
            var entries = Enum.Values.Keys.ToList();
            for (var i = 0; i < entries.Count; i++)
            {
                var key = entries[i];
                var description = Enum.Descriptions!.TryGetValue(key, out var desc) ? desc : "";

                gen.Write($"[{Enum.Name}.{key}]:");
                gen.Write($" \"{EscapeString(description)}\"", false);

                if (i < entries.Count - 1)
                {
                    gen.Write(",");
                    gen.NewLine();
                }
            }
        }, true);
    }

    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}
