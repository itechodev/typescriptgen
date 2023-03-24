using System;
using System.Linq;
using System.Text.Json;
using static System.Text.Json.JsonNamingPolicy;

namespace TypescriptGen
{
    public static class TsGenerator
    {
        public static string CamelCase(string s)
        {
            return JsonNamingPolicy.CamelCase.ConvertName(s);
        }

        // Return the name used to reference tsType
        public static string ToDefString(TsType tsType)
        {
            switch (tsType)
            {
                case TsArray tsArray:
                    return $"{ToDefString(tsArray.ElementType)}[]";
                case TsDictionary tsDictionary:
                    return $"Record<{DicType(tsDictionary.Key)}, {ToDefString(tsDictionary.Value)}>";
                case TsInterface tsInterface:
                    // if (tsInterface.Generics.Count > 0)
                    //     return $"{tsInterface.Name}<{string.Join(" ,", tsInterface.Generics.Select(g => g.Name))}";
                    return tsInterface.Name;
                case TsGenericType generic:
                    return generic.Name;
                case TsEnum @enum:
                    return @enum.Name;
                case TsFileType:
                    return "File";
                case TsPrimitive tsPrimitive:
                    return tsPrimitive.PrimitiveType switch
                    {
                        TsPrimitive.TsPrimitiveType.Boolean => "boolean",
                        TsPrimitive.TsPrimitiveType.Number => "number",
                        TsPrimitive.TsPrimitiveType.String => "string",
                        TsPrimitive.TsPrimitiveType.DateTime => "Date",
                        _ => throw new ArgumentOutOfRangeException()
                    };
                case TsVoid tsVoid:
                    return "void";
                case TsGenericReference genericReference:
                    var list = string.Join(", ", genericReference.Generics.Select(g => ToDefString(g)));
                    return ToDefString(genericReference.BaseType) + $"<{list}>";
                default:
                    throw new ArgumentOutOfRangeException(nameof(tsType));
            }
        }
        
        private static string DicType(NumberStringKey key)
        {
            return key == NumberStringKey.Number ? "number" : "string";
        }

        private static string FormatName(Type type)
        {
            // ActionResult'1 or IReadOnlyList'1
            if (type.IsGenericType)
            {
                var genericName = type.Name.Remove(type.Name.IndexOf('`'));
                return genericName + string.Join("", type.GetGenericArguments().Select(FormatName));
            }

            return type.Name;
        }
    }
}