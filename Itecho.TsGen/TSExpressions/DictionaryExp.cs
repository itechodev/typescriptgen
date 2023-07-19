namespace Itecho.TsGen.TSExpressions;

public class DictionaryExp : TsExp
{
    public Dictionary<TsExp, TsExp> Values { get; }

    public DictionaryExp(Dictionary<TsExp, TsExp> values)
    {
        Values = values;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}