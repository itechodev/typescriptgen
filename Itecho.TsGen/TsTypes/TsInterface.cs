using System.Reflection;

namespace Itecho.TsGen.TsTypes;

/// <summary>
/// represents an TS interface
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

    public TsInterface(string name, TsInterfaceMember[] members)
    {
        Name = name;
        Members = members;
    }
}