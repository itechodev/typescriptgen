namespace Itecho.TsGen.TsTypes;

public class TsImportTypeResolverItem
{
    public string Name { get; }
    public bool IsInterface { get;}

    public TsImportTypeResolverItem(string name, bool isInterface)
    {
        Name = name;
        IsInterface = isInterface;
    }
}

public class TsImportTypeResolver : TypeVisitor
{
    private readonly TsInterface? _originalInterface;
    private readonly List<TsImportTypeResolverItem> _list = new();

    public TsImportTypeResolver(TsInterface? originalInterface)
    {
        _originalInterface = originalInterface;
    }

    public List<TsImportTypeResolverItem> GetImports()
    {
        return _list
            .OrderBy(n => n.Name)
            .ToList();
    }

    protected override void VisitEnum(TsEnum @enum)
    {
        _list.Add(new TsImportTypeResolverItem(@enum.Name, false));
    }

    protected override void VisitInterface(TsInterface @interface)
    {
        if (_originalInterface == @interface)
        {
            base.VisitInterface(@interface);
        }
        else
        {
            // do not recurse
            // only interested in top level imports
            _list.Add(new TsImportTypeResolverItem(@interface.Name, true));
        }
    }
}
