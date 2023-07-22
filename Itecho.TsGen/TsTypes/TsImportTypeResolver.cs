namespace Itecho.TsGen.TsTypes;

public class TsImportTypeResolver
{
    private readonly List<string> _list = new();

    public List<string> GetImports(string excludeName)
    {
        return _list
            .Distinct()
            .Where(n => n != excludeName)
            .OrderBy(n => n)
            .ToList();
    }

    public void Resolve(IEnumerable<TsType> list)
    {
        foreach (var m in list)
        {
            Resolve(m);
        }
    }

    public void Resolve(TsType type)
    {
        switch (type)
        {
            case TsInterface @interface:
                _list.Add(@interface.Name);
                return;
            case TsGenericReference genericReference:
                Resolve(genericReference.ReferencedType);
                Resolve(genericReference.Parameters);
                return;
            case TsEnum @enum:
                _list.Add(@enum.Name);
                return;
            case TsArray array:
                Resolve(array.ElementType);
                return;
            case TsDictionary dictionary:
                Resolve(dictionary.Key);
                Resolve(dictionary.Value);
                return;
            case TsUnion union:
                Resolve(union.Types);
                return;
            case TsIntersection intersection:
                Resolve(intersection.Types);
                return;
        }
    }

}
