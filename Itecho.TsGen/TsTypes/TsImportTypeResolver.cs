namespace Itecho.TsGen.TsTypes;

public class TsImportTypeResolver : TypeVisitor
{
    private readonly TsInterface? _originalInterface;
    private readonly List<string> _list = new();

    public TsImportTypeResolver(TsInterface? originalInterface)
    {
        _originalInterface = originalInterface;
    }

    public List<string> GetImports()
    {
        return _list
            .OrderBy(n => n)
            .ToList();
    }

    protected override void VisitEnum(TsEnum @enum)
    {
        _list.Add(@enum.Name);
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
            _list.Add(@interface.Name);
        }
    }
}