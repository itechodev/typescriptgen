using Itecho.TsGen.TSExpressions;

namespace Itecho.TsGen;

public static class HttpClientFile
{
    public static TsFile Generate()
    {
        var file = new TsFile();

        file.Add(TsExp.Literal(@"import {type HttpHandler, type HttpDic} from './httpHandler';

const httpClient: HttpClient = {
    post: function <TReq, TRes>(url: string, params?: HttpDic | undefined, body?: TReq | FormData | undefined, headers?: HttpDic | undefined): Promise<TRes> {
        throw new Error('Function not implemented.');
    },
    get: function <TReq, TRes>(url: string, params?: HttpDic | undefined, body?: TReq | FormData | undefined, headers?: HttpDic | undefined): Promise<TRes> {
        throw new Error('Function not implemented.');
    },
    put: function <TReq, TRes>(url: string, params?: HttpDic | undefined, body?: TReq | FormData | undefined, headers?: HttpDic | undefined): Promise<TRes> {
        throw new Error('Function not implemented.');
    patch: function <TReq, TRes>(url: string, params?: HttpDic | undefined, body?: TReq | FormData | undefined, headers?: HttpDic | undefined): Promise<TRes> {
        throw new Error('Function not implemented.');
    },
    delete: function <TReq, TRes>(url: string, params?: HttpDic | undefined, body?: TReq | FormData | undefined, headers?: HttpDic | undefined): Promise<TRes> {
        throw new Error('Function not implemented.');
    }
}"));
        return file;
    }
}
