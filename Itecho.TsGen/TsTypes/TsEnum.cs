namespace Itecho.TsGen.TsTypes;

public class TsEnum : TsType
{
    public enum TsEnumValueType
    {
        Undefined,
        String,
        Number
    }
    public TsEnumValueType ValueType { get; set; }
    public Dictionary<string, int> Values { get; set; }

    public TsEnum(TsEnumValueType valueType, Dictionary<string, int> values)
    {
        ValueType = valueType;
        Values = values;
    }
}