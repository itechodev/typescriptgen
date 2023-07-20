namespace Itecho.TsGen.TSExpressions;

public class DictionaryEntry
{
    public TsExp Key { get; }
    public TsExp Value { get; }

    public DictionaryEntry(TsExp key, TsExp value)
    {
        Key = key;
        Value = value;
    }
}

public class DictionaryExp: TsExp 
{
    public IEnumerable<DictionaryEntry> Entries { get; }

    public DictionaryExp(IEnumerable<DictionaryEntry> entries)
    {
        Entries = entries;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}