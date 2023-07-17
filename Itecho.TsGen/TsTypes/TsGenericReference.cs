namespace Itecho.TsGen.TsTypes;

/// <summary>
/// A reference to an interface with generic parameters
/// ie: ReferencedType<Type1, Type2, Type3>
/// where ReferencedType is a TsInterface
/// </summary>
public class TsGenericReference : TsType
{
    public TsInterface ReferencedType { get; }
    public TsType[] GenericParameters { get; }

    public TsGenericReference(TsInterface referencedType, TsType[] genericParameters)
    {
        ReferencedType = referencedType;
        GenericParameters = genericParameters;
    }
}