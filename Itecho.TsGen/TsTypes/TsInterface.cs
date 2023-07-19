using System.Reflection;

namespace Itecho.TsGen.TsTypes;

/// <summary>
/// Represents an TS interface.
/// Typically converted from C# class and interfaces
/// Includes optional generics and inheritance
/// For a type that references a type with specific generic parameters, see TsGenericReference
/// </summary>
public class TsInterface : TsType
{
    public class TsInterfaceMember
    {
        public string Name { get; }
        public TsType Type { get; }

        public TsInterfaceMember(string name, TsType type)
        {
            Name = name;
            Type = type;
        }
    }

    public string Name { get; private set; }
    public TsInterfaceMember[] Members { get; private set; }
    public TsInterface? Extends { get; private set; }
    public TsGeneric[] Generics { get; private set; }

    public TsInterface()
    {
        Name = string.Empty;
        Members = Array.Empty<TsInterfaceMember>();
        Extends = null;
        Generics = Array.Empty<TsGeneric>();
    }

    public void CopyFrom(TsInterface other)
    {
        Name = other.Name;
        Members = other.Members;
        Extends = other.Extends;
        Generics = other.Generics;
    }

    public TsInterface(string name, TsInterfaceMember[] members, TsInterface? extends, TsGeneric[] generics)
    {
        Name = name;
        Members = members;
        Extends = extends;
        Generics = generics;
    }

    private static List<string> GetReferencedTypes(params TsType[] list)
    {
        var ret = new List<string>();
        foreach (var m in list)
        {
            switch (m)
            {
                case TsInterface @interface:
                    ret.Add(@interface.Name);
                    continue;
                case TsGenericReference genericReference:
                    ret.Add(genericReference.ReferencedType.Name);
                    // also add potential generic parameters
                    ret.AddRange(GetReferencedTypes(genericReference.Parameters));
                    continue;
                case TsEnum @enum:
                    ret.Add(@enum.Name);
                    continue;
                case TsArray array:
                    ret.AddRange(GetReferencedTypes(array.ElementType));
                    continue;
            }
        }

        return ret.Distinct().ToList();
    }

    public List<string> GetReferencedTypes()
    {
        return GetReferencedTypes(Members.Select(m => m.Type).ToArray())
            .OrderBy(n => n)
            .ToList();
    }
}