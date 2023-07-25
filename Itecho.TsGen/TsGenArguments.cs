namespace Itecho.TsGen;

public static class TsGenArguments
{
    public static List<string> Ignore { get; set; } = new();
    public static bool ExplicitReturns { get; set; }

    public static string ControllerFolder { get; set; } = "controllers";
    public static string InterfacesFolder { get; set; } = "interfaces";
}