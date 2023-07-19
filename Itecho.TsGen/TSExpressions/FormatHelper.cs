using System.Text.Json;

namespace Itecho.TsGen.TSExpressions;

public static class FormatHelper
{
    public static string CamelCase(string s)
    {
        return JsonNamingPolicy.CamelCase.ConvertName(s);
    }
}