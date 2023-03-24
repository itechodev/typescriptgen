namespace typescriptgen;

public class TsInterface
{
    // actual name of the corresponding C# class
    public string Name { get; set; } = string.Empty;
    public List<TsProperty> Properties { get; set; } = new();
}

public enum TsTypeCode
{
    Undefined,
    Null,
    Literal,

    String,
    Number,
    Boolean,
    Date,

    Array,
    Enum,

    Interface,
    // for now we can only generate a interface.

    Any
}

public class TsEnum
{
    public string Name { get; set; } = string.Empty;
    public List<TsEnumMember> Members { get; set; } = new();
}

public enum TsEnumValueType
{
    Undefined,
    String,
    Number
}

public class TsEnumMember
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public TsEnumValueType ValueType { get; set; }
}

// follow the convention of the C# type system
public class TsType
{
    public TsTypeCode TypeCode { get; set; }

    public bool IsNullable { get; set; }

    // specify the array element type if type code is Array
    public TsTypeCode ElementType { get; set; }

    // if the typeCode is Object or interface
    public TsInterface? Interface { get; set; }

    // options for ts enums
    public TsEnum? Enum { get; set; }
}

public class TsProperty
{
    public string Name { get; set; } = string.Empty;
    public TsType Type { get; set; } = new();
}