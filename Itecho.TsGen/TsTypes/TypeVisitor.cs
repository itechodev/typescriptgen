namespace Itecho.TsGen.TsTypes;

public class TypeVisitor
{
    private List<TsType> _visisted = new();


    public void Visit(IEnumerable<TsType> types)
    {
        foreach (var type in types)
        {
            Visit(type);
        }
    }

    public void Visit(TsType type)
    {
        // avoid infinite recursion with referencing types
        if (_visisted.Contains(type))
            return;

        _visisted.Add(type);

        switch (type)
        {
            case TsInterface @interface:
                VisitInterface(@interface);
                return;
            case TsGenericReference genericReference:
                VisitGenericReference(genericReference);
                return;
            case TsGeneric generic:
                VisitGeneric(generic);
                return;
            case TsBuildInType buildInType:
                VisitBuildIn(buildInType);
                return;
            case TsEnum @enum:
                VisitEnum(@enum);
                return;
            case TsArray array:
                VisitArray(array);
                return;
            case TsDictionary dictionary:
                VisitDictionary(dictionary);
                return;
            case TsTuple tuple:
                VisitTuple(tuple);
                return;
            case TsUnion union:
                VisitUnion(union);
                return;
            case TsIntersection intersection:
                VisitIntersection(intersection);
                return;
            case TsPrimitive primitive:
                VisitPrimitive(primitive);
                return;
            case TsVoid @void:
                VisitVoid(@void);
                return;
        }
    }

    protected virtual void VisitGeneric(TsGeneric generic)
    {
    }


    protected virtual void VisitArray(TsArray array)
    {
        Visit(array.ElementType);
    }

    protected virtual void VisitBuildIn(TsBuildInType array)
    {
    }

    private void VisitComposite(TsCompositeType compositeType)
    {
        foreach (var t in compositeType.Types)
        {
            Visit(t);
        }
    }

    protected virtual void VisitUnion(TsUnion union)
    {
        VisitComposite(union);
    }

    protected virtual void VisitTuple(TsTuple tuple)
    {
        VisitComposite(tuple);
    }

    protected virtual void VisitIntersection(TsIntersection intersection)
    {
        VisitComposite(intersection);
    }

    protected virtual void VisitDictionary(TsDictionary dictionary)
    {
        Visit(dictionary.Key);
        Visit(dictionary.Value);
    }

    protected virtual void VisitEnum(TsEnum @enum)
    {
    }

    protected virtual void VisitGenericReference(TsGenericReference genericReference)
    {
        Visit(genericReference.ReferencedType);
        foreach (var param in genericReference.Parameters)
        {
            Visit(param);
        }
    }

    protected virtual void VisitInterface(TsInterface @interface)
    {
        if (@interface.Extends != null)
        {
            Visit(@interface.Extends);
        }

        foreach (var g in @interface.Generics)
        {
            Visit(g);
        }

        foreach (var member in @interface.Members)
        {
            Visit(member.Type);
        }
    }

    protected virtual void VisitPrimitive(TsPrimitive primitive)
    {
    }

    protected virtual void VisitVoid(TsVoid @void)
    {
    }
}