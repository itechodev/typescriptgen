namespace Itecho.TsGen.TSExpressions;

public class ObjectAccessExp : TsExp
{
    public TsExp Object { get; }
    public string Member { get; }

    public ObjectAccessExp(TsExp o, string member)
    {
        Object = o;
        Member = member;
    }

    public override void Write(TsCodeGenerator gen)
    {
        Object.Write(gen);
        gen.Write("." + Member);
    }
}