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
    body?: object;
    // the binding of the body object representing [FromBody] and [FromForm] in C#
    binding?: 'body' | 'form';
    queryParams?: Record<string, unknown>;
    headers?: Record<string, unknown>;
}"));
        return file;
    }
}
