namespace Itecho.TsGen.TsTypes;

/// <summary>
/// Build in non-primitive types such as File, Promise etc/ 
/// </summary>
public class TsBuildInType : TsType
{
    public string BuildInType { get; set; }

    public TsBuildInType(string buildInType)
    {
        BuildInType = buildInType;
    }
}