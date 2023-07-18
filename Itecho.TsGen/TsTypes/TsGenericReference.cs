namespace Itecho.TsGen.TsTypes;

/// <summary>
/// A reference to an interface with generic parameters
/// ie: ReferencedType<Type1, Type2, Type3>
/// where ReferencedType is a TsInterface
/// </summary>
public class TsGenericReference : TsType
{
    public class TsGenericReferenceType
    {
        public string Name { get; set; }
        public TsInterface? Constraint { get; set; }
    
        public TsGenericReferenceType(string name, TsInterface? constraint)
        {
            Name = name;
            Constraint = constraint;
        }
    }
    
    public TsInterface ReferencedType { get; }
    public TsGenericReferenceType[] Parameters { get; }

    public TsGenericReference(TsInterface referencedType, TsGenericReferenceType[] parameters)
    {
        ReferencedType = referencedType;
        Parameters = parameters;
    }
}

