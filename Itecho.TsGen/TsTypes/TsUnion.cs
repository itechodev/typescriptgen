namespace Itecho.TsGen.TsTypes;

/// <summary>
/// types Union = type1 | type2 | type3
/// </summary>
public class TsUnion : TsCompositeType
{
    /// <summary>
    /// quick check if union contains null
    /// </summary>
    public bool ContainsNull { get; set; }

    public TsUnion(params TsType[] types) : base(types)
    {
        ContainsNull = types.OfType<TsPrimitive>().Any(t => t.Type == TsPrimitive.TsPrimitiveType.Null);
    }
}