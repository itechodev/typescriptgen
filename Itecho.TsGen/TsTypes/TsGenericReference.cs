namespace Itecho.TsGen.TsTypes;

/// <summary>
/// A reference to an interface with generic parameters
/// ie: ReferencedType<Type1, Type2, Type3>
/// where ReferencedType is a TsInterface
/// </summary>
public class TsGenericReference : TsType
{
    public TsType ReferencedType { get; }
    public TsType[] Parameters { get; }

    public TsGenericReference(TsType referencedType, TsType[] parameters)
    {
        ReferencedType = referencedType;
        Parameters = parameters;
    }
}