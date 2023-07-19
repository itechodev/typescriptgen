namespace Itecho.TsGen.TsTypes;

/// <summary>
/// type tuple = [type1, type2, type3]
/// </summary>
public class TsTuple : TsCompositeType
{
    public TsTuple(params TsType[] types) : base(types)
    {
    }
}