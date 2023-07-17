namespace Itecho.TsGen;

public static class TypeExtensions
{
    public static bool IsDictionary(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }

    public static bool IsArray(this Type type)
    {
        // string is IEnumerable<char> so we need to exclude it
        if (type == typeof(string))
            return false;

        return type.IsArray;
    }

    public static bool IsEnumerable(this Type type)
    {
        return type
            .GetInterfaces()
            .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    public static bool IsNullable(this Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }
}