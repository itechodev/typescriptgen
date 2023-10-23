namespace Itecho.TsGen.TSExpressions;

public class ArrayExp : TsExp
{
    public IEnumerable<TsExp> Entries { get; }
    public ArrayExp(IEnumerable<TsExp> entries)
    {
        Entries = entries;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write("[");
        foreach (var (entry, index) in Entries.WithIndex())
        {
            entry.Write(gen);
            if (index < Entries.Count() - 1)
            {
                gen.Write(", ");
            }
        }
        gen.Write("]");
    }
}
