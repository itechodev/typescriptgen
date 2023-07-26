using System.Reflection;
using Itecho.TsGen.TSExpressions;
using Itecho.TsGen.TsTypes;
using Microsoft.AspNetCore.Http.Features;

namespace Itecho.TsGen;

public static class TsGenerator
{


    public static void GenerateController(ControllerInfo controller, string outputPath)
    {
        if (controller.Actions.Count == 0)
        {
            return;
        }
        
        var tsFile = new TsFile();
        tsFile.Add(TsExp.Comment("eslint-disable", true));
        tsFile.Add(VersionInfo.GenerationNotice);

        tsFile.Add(TsExp.EmptyLine());
        // import the user customisable http client 
        tsFile.Add(TsExp.Import("../request", null,
            new ImportExp.NamedImport("webRequest", false),
            new ImportExp.NamedImport("fileRequest", false)));
        tsFile.Add(TsExp.EmptyLine());

        // import all references for this controller
        foreach (var import in controller.GetReferencedTypes())
        {
            tsFile.Add(TsExp.Import($"../{TsGenArguments.InterfacesFolder}/{FormatHelper.CamelCase(import)}", new ImportExp.NamedImport(import, true)));
            tsFile.Add(TsExp.EmptyLine());
        }

        tsFile.Add(TsExp.EmptyLine());

        var exportEntries = controller.Actions.Select(action => new DictionaryEntry(
            TsExp.Literal(FormatHelper.CamelCase(action.Name)),
            BuildControllerAction(controller, action))
        );

        tsFile.Add(TsExp.DefaultExport(TsExp.Dictionary(exportEntries)));

        tsFile.WriteToFile(Path.Combine(outputPath, TsGenArguments.ControllerFolder,
            FormatHelper.CamelCase(controller.Name)));
    }

    private static TsExp BuildControllerAction(ControllerInfo controller, ActionInfo action)
    {
        /*
         httpClient<T>(url: string, options?: HttpOptions): Promise<T>
         export interface HttpOptions {
            method?: 'get' | 'post' | 'put' | 'patch' | 'delete';
            body?: object | FormData;
            queryParams?: Record<string, unknown>;
            headers?: Record<string, unknown>;
        }
        */

        // var returnType = TsType.GenericReference(TsType.BuildIn("Promise"), action.ReturnType);

        var paramList = action.Parameters.Select(p =>
            new TsParameter(p.Name, p.Type));

        var options = new List<DictionaryEntry>();
        AddMethod(options, action.Kind);
        AddBody(options, action);
        AddQueryParams(options, action);
        AddHeaders(options, action);

        var clientParams = new List<TsExp>()
        {
            RouteHelper.BuildUrl(controller, action)
        };
        if (options.Any())
        {
            clientParams.Add(TsExp.Dictionary(options));
        }

        // If file is returned from GET action
        // only return the url tha will download the file
        // so that it can be freely used in different downloading options
        if (action.ReturnType is TsBuildInType { BuildInType: BuildInType.File })
        {
            return TsExp.Lambda(null, paramList,
                TsExp.Return(
                    TsExp.FunctionCall(TsExp.Literal("fileRequest"), null, clientParams.ToArray())
                ));
        }

        return TsExp.Lambda(null, paramList,
            TsExp.Return(
                TsExp.FunctionCall(TsExp.Literal("webRequest"), new List<TsType>
                    {
                        action.ReturnType
                    }, clientParams.ToArray()
                )
            ));
    }

    private static void AddHeaders(List<DictionaryEntry> options, ActionInfo action)
    {
        // handle [FromHeader}
        var headerParams = action.Parameters
            .Where(p => p.Kind == ActionParameterKind.Header)
            .Select(g => new DictionaryEntry(TsExp.Literal(g.Name)))
            .ToList();

        if (headerParams.Any())
        {
            options.Add(new DictionaryEntry(TsExp.Literal("headers"), TsExp.Dictionary(headerParams)));
        }
    }

    private static void AddQueryParams(List<DictionaryEntry> options, ActionInfo action)
    {
        // handle [FromQuery}
        var queryParams = action.Parameters
            .Where(p => p.Kind == ActionParameterKind.Query)
            .Select(g => new DictionaryEntry(TsExp.Literal(g.Name)))
            .ToList();
        if (queryParams.Any())
        {
            options.Add(new DictionaryEntry(TsExp.Literal("queryParams"), TsExp.Dictionary(queryParams)));
        }
    }

    private static void AddBody(List<DictionaryEntry> options, ActionInfo action)
    {
        // handle [FromBody] and [FromForm]
        var bodyParam =
            action.Parameters.SingleOrDefault(p => p.Kind is ActionParameterKind.Body or ActionParameterKind.Form);
        if (bodyParam != null)
        {
            options.Add(new DictionaryEntry(TsExp.Literal("body"), TsExp.Literal(bodyParam.Name)));
        }
    }

    private static void AddMethod(List<DictionaryEntry> options, ActionKind kind)
    {
        // { method: 'get'} is the default
        // for anything else we need to explicit pass it
        if (kind != ActionKind.Get)
        {
            options.Add(new DictionaryEntry(TsExp.Literal("method"), TsExp.String(kind.ToString().ToLower())));
        }
    }

    public static void GenerateInterface(TsInterface @interface, string outputPath)
    {
        var tsFile = new TsFile();
        tsFile.Add(VersionInfo.GenerationNotice);

        // import all interfaces referenced by this interface
        foreach (var import in @interface.GetReferencedTypes())
        {
            tsFile.Add(TsExp.Import($"./{FormatHelper.CamelCase(import)}", new ImportExp.NamedImport(import, true)));
            tsFile.Add(TsExp.EmptyLine());
        }

        tsFile.Add(TsExp.EmptyLine());
        tsFile.Add(TsExp.DefaultExport(TsExp.Interface(@interface)));
        
        tsFile.WriteToFile(Path.Combine(outputPath, TsGenArguments.InterfacesFolder, FormatHelper.CamelCase(@interface.Name)));
    }
    public static void GenerateEnum(TsEnum @enum, string outputPath)
    {
        var tsFile = new TsFile();
        tsFile.Add(VersionInfo.GenerationNotice);
        tsFile.Add(TsExp.EmptyLine());
        tsFile.Add(TsExp.Enum(@enum));
        tsFile.Add(TsExp.EmptyLine());
        tsFile.Add(TsExp.DefaultExport(TsExp.Literal(@enum.Name)));
        tsFile.WriteToFile(Path.Combine(outputPath, TsGenArguments.InterfacesFolder, FormatHelper.CamelCase(@enum.Name)));
    }
}