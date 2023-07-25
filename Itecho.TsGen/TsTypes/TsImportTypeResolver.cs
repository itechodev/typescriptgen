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
        base.VisitEnum(@enum);
    }

    protected override void VisitInterface(TsInterface @interface)
    {
        if (_originalInterface != @interface)
            _list.Add(@interface.Name);

        base.VisitInterface(@interface);
    }
}