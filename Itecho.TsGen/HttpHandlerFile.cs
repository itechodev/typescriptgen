using Itecho.TsGen.TSExpressions;

namespace Itecho.TsGen;

public static class HttpHandlerFile
{
    public static TsFile Generate()
    {
        var file = new TsFile();

        file.Add(VersionInfo.GenerationNotice);

        file.Add(TsExp.Literal(@"
// data carrying dictionary for data payloads and headers
export type HttpDic = Record<string, unknown>;

// signature for all http verbs 
export type HttpPayloadRequest = <TReq, TRes>(url: string, params?: HttpDic, body?: FormData | TReq, headers?: HttpDic) => Promise<TRes>;

// HttpClient interface defines methods corresponding to HTTP verbs
export interface HttpHandler {
    post: HttpPayloadRequest;
    get: HttpPayloadRequest;
    put: HttpPayloadRequest;
    patch: HttpPayloadRequest;
    delete: HttpPayloadRequest;
}
)"));
        return file;
    }
}