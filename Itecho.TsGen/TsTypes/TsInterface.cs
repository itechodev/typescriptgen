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

    public string Name { get; }
    public TsInterfaceMember[] Members { get; }

    public TsInterface? Extends { get; }

    public TsInterface(string name, TsInterfaceMember[] members, TsInterface? extends)
    {
        Name = name;
        Members = members;
        Extends = extends;
    }
}