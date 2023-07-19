namespace Itecho.TsGen.TsTypes;

public class TsPrimitive : TsType
{
    public enum TsPrimitiveType
    {
        Undefined,
        Null,
        String,
        Number,
        Boolean,
        Date,
        Unknown,
        Any
    }

    public TsPrimitiveType Type { get; set; }

    public TsPrimitive(TsPrimitiveType type)
    {
        Type = type;
    }
}