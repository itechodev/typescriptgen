using Itecho.TsGen.TSExpressions;

namespace Itecho.TsGen.TsTypes;

public class TsPrimitive : TsType
{
    public enum TsPrimitiveType
    {
        Undefined, Null, String, Number, Boolean, Date, Unknown, Any
    }

    public TsPrimitiveType Type { get; set; }

    public TsPrimitive(TsPrimitiveType type)
    {
        Type = type;
    }

    public static TsPrimitive Undefined() => new(TsPrimitiveType.Undefined);
    public static TsPrimitive Null() => new(TsPrimitiveType.Null);
    public static TsPrimitive String() => new(TsPrimitiveType.String);
    public static TsPrimitive Number() => new(TsPrimitiveType.Number);
    public static TsPrimitive Boolean() => new(TsPrimitiveType.Boolean);
    public static TsPrimitive Date() => new(TsPrimitiveType.Date);
    public static TsPrimitive Unknown() => new(TsPrimitiveType.Unknown);
    public static TsPrimitive Any() => new(TsPrimitiveType.Any);
}
