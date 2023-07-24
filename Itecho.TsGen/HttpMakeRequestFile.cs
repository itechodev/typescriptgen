using Itecho.TsGen.TSExpressions;

namespace Itecho.TsGen;

public static class HttpMakeRequestFile
{
    public static TsFile Generate()
    {
        var file = new TsFile();

        file.Add(TsExp.Literal(@"import {type RequestOptions} from './requestOptions';

//  (url: string, options?: HttpOptions) => Promise<T>;
export default function makeRequest<T>(url: string, options?: RequestOptions): Promise<T> {
    throw new Error('Function not implemented.');
}"));
        return file;
    }
}