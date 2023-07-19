namespace Itecho.TsGen.Ts.Types;

public class TsDictionary : TsType
{
    public TsType Key { get; }
    public TsType Value { get; }

    public TsDictionary(TsType key, TsType value)
    {
        Key = key;
        Value = value;
    }
}