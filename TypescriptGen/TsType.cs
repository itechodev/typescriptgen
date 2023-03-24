using System;
using System.Collections.Generic;

namespace TypescriptGen
{
    // base of all TypeScript types
    public abstract class TsType
    {
        public bool Nullable { get; set; }
    }

    // For functions returning void.
    public class TsVoid : TsType
    {
    }

    public class TsFileType : TsType
    {

    }

    public class TsArray : TsType
    {
        public TsType ElementType { get; set; }

        public TsArray(TsType elementType)
        {
            ElementType = elementType;
        }
    }

    public class TsPrimitive : TsType
    {
        public enum TsPrimitiveType
        {
            Boolean,
            Number,
            String,
            DateTime
        }

        public TsPrimitiveType PrimitiveType { get; }

        public TsPrimitive(TsPrimitiveType primitiveType, bool nullable)
        {
            PrimitiveType = primitiveType;
            Nullable = nullable;
        }
    }

    public class TsGenericType : TsType
    {
        public string Name { get; }

        public TsGenericType(string name)
        {
            Name = name;
        }
    }


    public class TsEnum : TsType
    {
        public Type ClrType { get; }
        public string Name { get; set; } = string.Empty;
        public List<KeyValuePair<string, string>> Options { get; set; }
        public NumberStringKey Key { get; set; }

        public TsEnum(Type clrType, string name, List<KeyValuePair<string, string>> options, NumberStringKey key)
        {
            ClrType = clrType;
            Name = name;
            Options = options;
            Key = key;
        }
    }

    /// <summary>
    /// class PaginationResponse<T, Q, R> where 
    /// </summary>
    public class TsGenericUsage
    {
        public string Name { get; set; } = string.Empty;
        public TsInterface? Constraint { get; set; }

        public TsGenericUsage(string name, TsInterface? constraint)
        {
            Name = name;
            Constraint = constraint;
        }
    }

    /// <summary>
    /// Reference a generic type.
    /// ie. type: PaginationResponse<Client>
    /// </summary>
    public class TsGenericReference : TsType
    {
        public TsInterface BaseType { get; set; }
        public List<TsType> Generics { get; set; }

        public TsGenericReference(TsInterface baseType, List<TsType> generics)
        {
            BaseType = baseType;
            Generics = generics;
        }
    }

    public class TsInterface : TsType
    {
        // Used to uniquely identify the type
        public Type ClrType { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<TsInterfaceMember> Members = new();
        public TsInterface? Inherit { get; set; } = null;
        public List<TsGenericUsage> Generics { get; set; } = new();

        public TsInterface(Type clrType)
        {
            ClrType = clrType;
        }
    }

    // Keys in JS/TS can only be a number or string
    public enum NumberStringKey
    {
        String,
        Number
    }

    public class TsDictionary : TsType
    {
        public NumberStringKey Key { get; set; }
        public TsType Value { get; set; }

        public TsDictionary(NumberStringKey key, TsType value)
        {
            Value = value;
            Key = key;
        }
    }

    public class TsInterfaceMember
    {
        public string Name { get; }
        public TsType TsType { get; }
        public bool Nullable { get; }
        public bool Optional { get; }

        public TsInterfaceMember(string name, TsType tsType, bool nullable, bool optional)
        {
            Name = name;
            TsType = tsType;
            Nullable = nullable;
            Optional = optional;
        }
    }
}