namespace Itecho.TsGen.Ts.Types;

/// <summary>
/// Types used in generics
/// interface Data<T> { data: T }
/// </summary>
public class TsGeneric: TsType
{
    public string Name { get; set; }

    public TsGeneric(string name)
    {
        Name = name;
    }
}