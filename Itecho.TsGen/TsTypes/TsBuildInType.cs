namespace Itecho.TsGen.TsTypes;

public enum BuildInType
{
    File
}

/// <summary>
/// Build in non-primitive types such as File, Promise etc/ 
/// </summary>
public class TsBuildInType : TsType
{
    
    public BuildInType BuildInType { get; set; }

    public TsBuildInType(BuildInType buildInType)
    {
        BuildInType = buildInType;
    }
}