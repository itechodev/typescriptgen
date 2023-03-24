using System;
using System.Collections.Generic;
using System.Linq;

namespace TypescriptGen
{
    public static class TypeExtensions
    {
        public static bool IsDictionary(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }
        
        public static bool IsArray(this Type type)
        {
            if (type == typeof(string))
                return false;

            return type.IsArray;
        }

        public static bool IsList(this Type type)
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
}