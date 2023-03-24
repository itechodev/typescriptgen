using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace TypescriptGen
{
    public class TsConverter
    {
        public List<TsInterface> Interfaces { get; } = new();
        public List<TsEnum> Enums { get; } = new();

        private static bool IsNumeric(TypeCode code)
        {
            return code is TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16
                or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single
                or TypeCode.Double or TypeCode.Decimal;
        }

        private TsType ConvertObject(Type type, bool nullable)
        {
            // First check dictionary then array because Dictionary inherits from IEnumerable
            if (type.IsDictionary())
            {
                return ConvertDictionary(type);
            }

            if (type.IsArray())
            {
                return new TsArray(ConvertType(type.GetElementType()))
                {
                    Nullable = nullable
                };
            }

            if (type.IsList())
            {
                return new TsArray(ConvertType(type.GetGenericArguments()[0]))
                {
                    Nullable = nullable
                };
            }

            // Convert ActionResult<T> to T 
            if (type.IsGenericType)
            {
                return ConvertGenericType(type, nullable);
            }

            // ContainsGenericParameters after array.
            // typeof(T[]).ContainsGenericParameters == true
            if (type.ContainsGenericParameters)
                return new TsGenericType(type.Name);


            // check if object already exists
            return Interfaces.Find(i => i.ClrType.FullName == type.FullName) ?? ConvertTsInterface(type, nullable);
        }

        private TsType ConvertGenericType(Type type, bool nullable)
        {
            var genericTypedef = type.GetGenericTypeDefinition();
            if (genericTypedef == typeof(Nullable<>))
            {
                return ConvertType(type.GenericTypeArguments[0], true);
            }
            
            if (genericTypedef == typeof(ActionResult<>) || genericTypedef == typeof(Task<>))
            {
                return ConvertType(type.GenericTypeArguments[0]);
            }

            // BaseClass<GenericSubstitute>
            // 3 types to add: BaseClass, GenericSubstitute and TsGenericReference: BaseClass<GenericSubstitute> 

            // Firs the base class, BaseClass<T,..> {}
            var geneticType = ConvertTsInterface(genericTypedef, nullable);

            // and then for each for the genetic arguments
            var genericArguments = type.GenericTypeArguments
                .Select(t => ConvertType(t))
                .ToList();

            // return the reference to this generic class
            return new TsGenericReference(geneticType, genericArguments);
        }

        private TsInterface ConvertTsInterface(Type type, bool nullable)
        {
            var generics = type.IsGenericType
                ? type.GetGenericArguments()
                    .Select(g => new TsGenericUsage(g.Name, null)).ToList()
                : new List<TsGenericUsage>();


            var inherited = ConvertType(type.BaseType);

            var ret = new TsInterface(type)
            {
                Name = FormatName(type.Name),
                Generics = generics,
                Inherit = inherited as TsInterface,
                Nullable = false
            };

            Interfaces.Add(ret);

            ret.Members = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                .Select(p =>
                {
                    var propertyNullable = p.PropertyType.IsNullable() || NullableHelper.IsNullable(p);
                    var json = p.GetCustomAttribute<JsonPropertyAttribute>();
                    var propName = json?.PropertyName ?? p.Name;
                    return new TsInterfaceMember(propName, ConvertType(p.PropertyType, propertyNullable), propertyNullable, false);
                })
                .ToList();
            
            return ret;
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
            return index == -1 ? typeName : typeName.Substring(0, index);
        }

        private TsType ConvertDictionary(Type type)
        {
            var args = type.GetGenericArguments();
            if (args.Length != 2)
                throw new ArgumentException("Dictionary should have 2 generic arguments");

            var keyType = Type.GetTypeCode(args[0]);
            if (keyType == TypeCode.String)
                return new TsDictionary(NumberStringKey.String,
                    ConvertType(args[1]));

            if (IsNumeric(keyType))
                return new TsDictionary(NumberStringKey.Number,
                    ConvertType(args[1]));

            throw new ArgumentException("Dictionary key must be a string or number.");
        }

        private static TsEnum ConvertEnum(Type type)
        {
            var t = Type.GetTypeCode(type.GetEnumUnderlyingType());
            var optionNames = type.GetEnumNames();
            var optionValue = type.GetEnumValues();

            var options = optionNames.Select((n, i) =>
                new KeyValuePair<string, string>(n, optionValue.GetValue(i)!.ToString())).ToList();
            
            return new TsEnum(type, type.Name, options, TsConverter.IsNumeric(Type.GetTypeCode(type)) ? NumberStringKey.Number : NumberStringKey.String);
        }

        public TsType ConvertType(Type type, bool nullable = false)
        {
            if (type == null || type == typeof(object) || type == typeof(ActionResult))
                return new TsVoid();
            
            if (type == typeof(IFormFile))
            {
                // type.GetCustomAttribute<NullableContextAttribute>();
                
                return new TsFileType()
                {
                    Nullable = type.IsNullable() || nullable 
                };
            }
            
            // GetTypeCode of enum is the underlying value like int32 or string
            // have to check before the GetTypeCode switch
            if (type.IsEnum)
            {
                var found = Enums.Find(i => i.ClrType.FullName == type.FullName);
                if (found != null)
                    return found;
            
                found = ConvertEnum(type);
                found.Nullable = nullable;
                Enums.Add(found);
                return found;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return new TsVoid();
                case TypeCode.Object:
                    return ConvertObject(type, nullable);
                case TypeCode.Boolean:
                    return new TsPrimitive(TsPrimitive.TsPrimitiveType.Boolean, nullable || type.IsNullable());
                case TypeCode.DateTime:
                    return new TsPrimitive(TsPrimitive.TsPrimitiveType.DateTime, nullable || type.IsNullable());
                case TypeCode.String:
                case TypeCode.Char:
                    return new TsPrimitive(TsPrimitive.TsPrimitiveType.String, nullable || type.IsNullable());
                default:
                    // Anything else is a number
                    return new TsPrimitive(TsPrimitive.TsPrimitiveType.Number, nullable || type.IsNullable());
            }
        }
    }
}