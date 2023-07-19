namespace Itecho.TsGen.TSExpressions;

public class ObjectAccessExp : TsExp
{
    public TsExp Object { get; }
    public string Access { get; }

    public ObjectAccessExp(TsExp o, string access)
    {
        Object = o;
        Access = access;
    }

    public override void Write(TsCodeGenerator gen)
    {
    }
}