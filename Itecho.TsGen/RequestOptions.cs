using Itecho.TsGen.TSExpressions;

namespace Itecho.TsGen;

public static class RequestOptions
{
    public static TsFile Generate()
    {
        var file = new TsFile();

        file.Add(VersionInfo.GenerationNotice);

        file.Add(TsExp.Literal(@"
export interface RequestOptions {
    method?: 'get' | 'post' | 'put' | 'patch' | 'delete';
    body?: object | FormData;
    queryParams?: Record<string, unknown>;
    headers?: Record<string, unknown>;
}"));
        return file;
    }
}
