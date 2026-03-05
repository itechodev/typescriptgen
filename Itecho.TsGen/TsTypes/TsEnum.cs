namespace Itecho.TsGen.TsTypes;

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
    public Dictionary<string, string>? Descriptions { get; }

    public bool HasDescriptions => Descriptions != null && Descriptions.Count > 0;

    public TsEnum(string name, TsEnumValueType valueType, Dictionary<string, int> values, Dictionary<string, string>? descriptions = null)
    {
        Name = name;
        ValueType = valueType;
        Values = values;
        Descriptions = descriptions;
    }
}