using Itecho.TsGen.TsTypes;
using Microsoft.AspNetCore.Mvc;

namespace Itecho.TsGen;

public static class TsConverter
{
    public static readonly Dictionary<Type, TsType> Cache = new();

    private static TsType AddCache(Type type, TsType tsType)
    {
        Cache.TryAdd(type, tsType);
        return tsType;
    }

    public static TsType Convert(Type type)
    {
        if (Cache.TryGetValue(type, out var convert))
            return convert;

        // GetTypeCode of enum is the underlying value like int32 or string
        // have to check before the GetTypeCode switch
        if (type.IsEnum)
        {
            return AddCache(type, ConvertEnum(type));
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Object:
                return AddCache(type, ConvertNonPrimitive(type));
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
                return new TsPrimitive(TsPrimitive.TsPrimitiveType.Number);
            case TypeCode.Empty:
                return new TsPrimitive(TsPrimitive.TsPrimitiveType.Undefined);
            case TypeCode.DBNull:
                return new TsPrimitive(TsPrimitive.TsPrimitiveType.Null);
            case TypeCode.Boolean:
                return new TsPrimitive(TsPrimitive.TsPrimitiveType.Boolean);
            case TypeCode.DateTime:
                return new TsPrimitive(TsPrimitive.TsPrimitiveType.Date);
            case TypeCode.Char:
            case TypeCode.String:
                return new TsPrimitive(TsPrimitive.TsPrimitiveType.String);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static TsEnum ConvertEnum(Type type)
    {
        var names = Enum.GetNames(type);
        // the underlying type for all enums are numeric
        var values = Enum.GetValues(type).Cast<int>();
        var options = names.Zip(values).ToDictionary(t => t.First, t => t.Second);

        // for now all enums are strings
        return new TsEnum(TsEnum.TsEnumValueType.String, options);
    }

    private static TsType ConvertNonPrimitive(Type type)
    {
        // controllers returning generic object or ActionResult<T> will be converted to unknown in TS
        // unknown almost better than any as you need safeguards before accessing   
        if (type == typeof(object) || type == typeof(ActionResult))
            return new TsPrimitive(TsPrimitive.TsPrimitiveType.Unknown);

        // convert IFormFile to File in TS, which is build in
        if (type == typeof(IFormFile))
        {
            return new TsBuildInType(TsBuildInType.BuildInTypes.File);
        }

        // First check dictionary then array because Dictionary inherits from IEnumerable
        if (type.IsDictionary())
        {
            return ConvertDictionary(type);
        }

        if (type.IsArray())
        {
            return new TsArray(Convert(type.GetElementType()!));
        }

        if (type.IsEnumerable())
        {
            return new TsArray(Convert(type.GetGenericArguments()[0]));
        }

        if (type.IsClass)
        {
            return ConvertClass(type);
        }

        // Convert ActionResult<T> to T 
        // if (type.IsGenericType)
        // {
        //     return ConvertGenericType(type, nullable);
        // }

        // ContainsGenericParameters after array.
        // typeof(T[]).ContainsGenericParameters == true
        // if (type.ContainsGenericParameters)
        //     return new TsGenericType(type.Name);

        return new TsPrimitive(TsPrimitive.TsPrimitiveType.Unknown);
    }

    /// <summary>
    /// Strip the ` after a type name. The number after the ` indicate the number of generic arguments a type have.
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    private static string FormatName(string typeName)
    {
        //   "PaginationResponse`1"
        var index = typeName.IndexOf('`');
        return index == -1 ? typeName : typeName[..index];
    }


    private static TsType ConvertClass(Type type)
    {
        // var generics = type.IsGenericType
        //     ? type.GetGenericArguments()
        //         .Select(g => new TsGenericUsage(g.Name, null)).ToList()
        //     : new List<TsGenericUsage>();


        if (type.BaseType != null)
        {
            var inherited = Convert(type.BaseType);
        }

        return new TsInterface(FormatName(type.Name), Array.Empty<TsInterface.TsInterfaceMember>());
        // {
        //     // Name = ,
        //     Generics = generics,
        //     Inherit = inherited as TsInterface,
        //     Nullable = false
        // };
        //
        // Interfaces.Add(ret);
        //
        // ret.Members = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        //     .Select(p =>
        //     {
        //         var propertyNullable = p.PropertyType.IsNullable() || NullableHelper.IsNullable(p);
        //         var json = p.GetCustomAttribute<JsonPropertyAttribute>();
        //         var propName = json?.PropertyName ?? p.Name;
        //         return new TsInterfaceMember(propName, ConvertType(p.PropertyType, propertyNullable), propertyNullable,
        //             false);
        //     })
        //     .ToList();
        //
        // return ret;
    }

    private static TsType ConvertDictionary(Type type)
    {
        var args = type.GetGenericArguments();
        if (args.Length != 2)
            throw new ArgumentException("Dictionary should have 2 generic arguments");

        return new TsDictionary(Convert(args[0]), Convert(args[1]));
    }
}