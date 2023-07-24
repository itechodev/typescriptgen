using System.Reflection;
using Itecho.TsGen.TsTypes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Itecho.TsGen;

public static class TsConverter
{
    public static readonly Dictionary<Type, TsType> Cache = new();


    private static TsInterface AddEmptyInterface(Type type)
    {
        // add empty interface and alter fill the members
        // used to prevent recursion and avoid duplicate visits
        return SetCache(type, new TsInterface());
    }

    private static T SetCache<T>(Type type, T tsType) where T : TsType
    {
        // could be already added by TsInterface
        Cache.TryAdd(type, tsType);
        return tsType;
    }

    // nullable flag is used to indicate nullability of the type
    // is being dig out in the class definition and the type alone cannot be used alone
    // using a combination of the property type, declaring type and runtime generated attributes
    public static TsType Convert(Type type, bool nullable = false)
    {
        if (Cache.TryGetValue(type, out var convert))
            return HandleNullable(convert, type, nullable);

        var converted = ConvertType(type);
        return HandleNullable(SetCache(type, converted), type, nullable);
    }

    // nullables are not always stored with the type
    // therefore we cannot cache the type alone
    private static TsType HandleNullable(TsType ret, Type type, bool nullable = false)
    {
        if (!nullable && !type.IsNullable()) return ret;
        // union already contains null
        // by Nullable<T> generic check
        if (ret is not TsUnion tsUnion) return new TsUnion(ret, new TsPrimitive(TsPrimitive.TsPrimitiveType.Null));

        if (tsUnion.ContainsNull)
            return tsUnion;

        // add null to union type
        var t = tsUnion.Types.Append(new TsPrimitive(TsPrimitive.TsPrimitiveType.Null));
        return new TsUnion(t.ToArray());
    }

    private static TsType ConvertType(Type type)
    {
        if (type.IsGenericParameter)
        {
            return new TsGeneric(type.Name);
        }

        // GetTypeCode of enum is the underlying value like int32 or string
        // have to check before the GetTypeCode switch
        if (type.IsEnum)
        {
            return ConvertEnum(type);
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Object:
                return ConvertNonPrimitive(type);
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

    private static TsType ConvertEnum(Type type)
    {
        var names = Enum.GetNames(type);
        // the underlying type for all enums are numeric
        var values = Enum.GetValues(type).Cast<int>();
        var options = names.Zip(values).ToDictionary(t => t.First, t => t.Second);
        // for now all enums are strings
        return new TsEnum(type.Name, TsEnum.TsEnumValueType.String, options);
    }

    private static TsType ConvertNonPrimitive(Type type)
    {
        // controllers returning generic object or ActionResult<T> will be converted to unknown in TS
        // unknown almost better than any as you need safeguards before accessing   
        if (type == typeof(object) || type == typeof(ActionResult))
            return new TsPrimitive(TsPrimitive.TsPrimitiveType.Unknown);

        // convert IFormFile to File in TS, which is build in
        // mostly used in requests
        if (type == typeof(IFormFile))
        {
            return new TsBuildInType(BuildInType.File);
        }

        // or when controller returns FileContentResult or FileStreamResult
        // which both inherits from FileResult
        // mostly used in responses
        if (typeof(FileResult).IsAssignableFrom(type))
        {
            return new TsBuildInType(BuildInType.File);
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


        if (type.IsGenericType && type != type.GetGenericTypeDefinition())
        {
            return ConvertGeneric(type);
        }

        if (type.IsClass)
        {
            return ConvertClass(type);
        }

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

    private static TsType ConvertGeneric(Type type)
    {
        // check if we can reduce the type to a lower level
        // e.g.ActionResult<T> is the same as T.
        // we don't need to convert ActionResult or Task
        var def = type.GetGenericTypeDefinition();


        if (def.IsTuple())
        {
            return new TsTuple(type.GenericTypeArguments.Select(g => Convert(g)).ToArray());
        }

        if (def == typeof(ActionResult<>))
        {
            if (type.GenericTypeArguments.Length == 0)
                return new TsPrimitive(TsPrimitive.TsPrimitiveType.Unknown);

            return Convert(type.GenericTypeArguments[0]);
        }

        // Any class that implements ActionResult FileContentResult
        if (typeof(ActionResult).IsAssignableFrom(def))
        {
            return new TsPrimitive(TsPrimitive.TsPrimitiveType.Unknown);
        }

        if (def == typeof(Nullable<>))
        {
            return new TsUnion(
                Convert(type.GenericTypeArguments[0]),
                new TsPrimitive(TsPrimitive.TsPrimitiveType.Null)
            );
        }


        var defType = AddEmptyInterface(type);
        // if type is a generic ie: PaginationResponse<Dog>
        // then we need to convert PaginationResponse<T> and Dog
        // and return the reference to PaginationResponse with filled generic Dog.
        // type.GetGenericArguments() == [Dog]
        var genericsArgs = type
            .GetGenericArguments()
            .Select(p => p.IsGenericParameter ? new TsGeneric(p.Name) : Convert(p))
            .ToArray();

        // type.GetGenericTypeDefinition() == PaginationResponse`1
        // type.GetGenericTypeDefinition().GetGenericArguments() == [T]
        var from = Convert(def);
        if (from is TsInterface inter)
        {
            defType.CopyFrom(inter);
        }

        return new TsGenericReference(defType, genericsArgs);
    }

    private static TsType ConvertClass(Type type)
    {
        var ret = AddEmptyInterface(type);

        var extends = type.BaseType != null && type.BaseType != typeof(object)
            ? Convert(type.BaseType)
            : null;

        var members = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
            .Select(p =>
            {
                var json = p.GetCustomAttribute<JsonPropertyAttribute>();
                var name = json?.PropertyName ?? p.Name;
                return new TsInterface.TsInterfaceMember(name, Convert(p.PropertyType, NullableHelper.IsNullable(p)));
            })
            .ToArray();

        var generics = type
            .GetGenericArguments()
            .Select(g => new TsGeneric(g.Name))
            .ToArray();

        // copy values to keep the same reference
        ret.CopyFrom(new TsInterface(FormatName(type.Name), members, extends as TsInterface, generics));
        // dont call setCache here, it's already added to the caches
        return ret;
    }

    private static TsType ConvertDictionary(Type type)
    {
        var args = type.GetGenericArguments();
        if (args.Length != 2)
            throw new ArgumentException("Dictionary should have 2 generic arguments");

        return new TsDictionary(Convert(args[0]), Convert(args[1]));
    }
}