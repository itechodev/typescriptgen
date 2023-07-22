using Itecho.TsGen.TSExpressions;

namespace Itecho.TsGen;

public static class HttpClientFile
{
    public static TsFile Generate()
    {
        var file = new TsFile();

        file.Add(TsExp.Literal(@"import {type HttpOptions} from './httpHandler';

//  (url: string, options?: HttpOptions) => Promise<T>;
export default function httpClient<T>(url: string, options?: HttpOptions): Promise<T> {
    throw new Error('Function not implemented.');
}"));
        return file;
    }
}
