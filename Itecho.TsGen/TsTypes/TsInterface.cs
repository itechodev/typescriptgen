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

    private static void GetReferenceType(List<string> ret, IEnumerable<TsType> list)
    {
        foreach (var m in list)
        {
            GetReferenceType(ret, m);
        }
    }

    private static void GetReferenceType(List<string> ret, TsType type)
    {
        switch (type)
        {
            case TsInterface @interface:
                ret.Add(@interface.Name);
                return;
            case TsGenericReference genericReference:
                ret.Add(genericReference.ReferencedType.Name);
                // also add potential generic parameters
                GetReferenceType(ret, genericReference.Parameters);
                return;
            case TsEnum @enum:
                ret.Add(@enum.Name);
                return;
            case TsArray array:
                GetReferenceType(ret, array.ElementType);
                return;
            case TsDictionary dictionary:
                GetReferenceType(ret, dictionary.Key);
                GetReferenceType(ret, dictionary.Value);
                return;
            case TsUnion union:
                GetReferenceType(ret, union.Types);
                return;
            case TsIntersection intersection:
                GetReferenceType(ret, intersection.Types);
                return;
        }
    }
    
    public List<string> GetReferencedTypes()
    {
        var list = new List<string>();
        if (Extends != null)
            list.Add(Extends.Name);

        GetReferenceType(list, Members.Select(m => m.Type));

        return list
            .OrderBy(n => n)
            .ToList();
    }
}