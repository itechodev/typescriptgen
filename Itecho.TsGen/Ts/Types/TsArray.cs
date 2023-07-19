namespace Itecho.TsGen.Ts.Types;

public class TsArray : TsType
{
    public TsType ElementType { get; set; }

    public TsArray(TsType elementType)
    {
        ElementType = elementType;
    }
}