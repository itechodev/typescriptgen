namespace Itecho.TsGen.TsTypes;

public class TsGenericTypeResolver : TypeVisitor
{
    public List<string> List { get;  } = new();

    protected override void VisitGeneric(TsGeneric generic)
    {
        if (!List.Contains(generic.Name))
            List.Add(generic.Name);
            
        base.VisitGeneric(generic);
    }
}