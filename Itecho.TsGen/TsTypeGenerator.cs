using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen;

public static class TsTypeGenerator
{
    public static string Generate(TsType type)
    {
        return type switch
        {
            TsArray array => $"{Generate(array.ElementType)}[]",
            TsBuildInType => "File",
            TsIntersection intersection => string.Join(" & ", intersection.Types.Select(Generate)),
            TsTuple tuple => $"[{string.Join(", ", tuple.Types.Select(Generate))}]",
            TsUnion union => string.Join(" | ", union.Types.Select(Generate)),
            TsDictionary dictionary =>
                $"Record<{Generate(dictionary.Key)}, {Generate(dictionary.Value)}>",
            TsEnum @enum => @enum.Name,
            TsGeneric generic => generic.Name,
            TsGenericReference genericReference =>
                $"{genericReference.ReferencedType.Name}<{string.Join(", ", genericReference.Parameters.Select(Generate))}>",
            TsInterface @interface => @interface.Name,
            TsPrimitive primitive => GeneratePrimitive(primitive),
            TsVoid @void => "void",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private static string GeneratePrimitive(TsPrimitive primitive)
    {
        return primitive.Type switch
        {
            TsPrimitive.TsPrimitiveType.Undefined => "undefined",
            TsPrimitive.TsPrimitiveType.Null => "null",
            TsPrimitive.TsPrimitiveType.String => "string",
            TsPrimitive.TsPrimitiveType.Number => "number",
            TsPrimitive.TsPrimitiveType.Boolean => "boolean",
            TsPrimitive.TsPrimitiveType.Date => "Date",
            TsPrimitive.TsPrimitiveType.Unknown => "unknown",
            TsPrimitive.TsPrimitiveType.Any => "any",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}