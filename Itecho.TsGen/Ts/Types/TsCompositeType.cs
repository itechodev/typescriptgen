namespace Itecho.TsGen.Ts.Types;

/// <summary>
/// Base for composite types such as union, intersection, tuples
/// </summary>
public abstract class TsCompositeType : TsType
{
    public TsType[] Types { get; }

    public TsCompositeType(params TsType[] types)
    {
        Types = types;
    }
}