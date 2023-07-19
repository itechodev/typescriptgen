namespace Itecho.TsGen.Ts.Types;

public class TsEnum : TsType
{
    public enum TsEnumValueType
    {
        Undefined,
        String,
        Number
    }

    public string Name { get; }
    public TsEnumValueType ValueType { get; }
    public Dictionary<string, int> Values { get; }

    public TsEnum(string name, TsEnumValueType valueType, Dictionary<string, int> values)
    {
        Name = name;
        ValueType = valueType;
        Values = values;
    }
}