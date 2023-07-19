namespace Itecho.TsGen.TsTypes;

public class TsArray : TsType
{
    public TsType ElementType { get; set; }

    public TsArray(TsType elementType)
    {
        ElementType = elementType;
    }
}