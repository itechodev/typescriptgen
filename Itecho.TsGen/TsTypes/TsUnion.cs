namespace Itecho.TsGen.TsTypes;

/// <summary>
/// types Union = type1 | type2 | type3
/// </summary>
public class TsUnion : TsCompositeType
{
    public TsUnion(params TsType[] types) : base(types)
    {
    }
}