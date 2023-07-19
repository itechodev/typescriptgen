namespace Itecho.TsGen.Ts.Types;

/// <summary>
/// Build in non-primitive types such as File 
/// </summary>
public class TsBuildInType : TsType
{
    public enum BuildInTypes
    {
        File,
    }

    public BuildInTypes BuildInType { get; set; }

    public TsBuildInType(BuildInTypes buildInType)
    {
        BuildInType = buildInType;
    }
}