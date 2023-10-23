using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
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

        // import all references for this controller
        foreach (var import in controller.GetReferencedTypes())
        {
            tsFile.Add(TsExp.Import($"../{TsGenArguments.InterfacesFolder}/{FormatHelper.CamelCase(import.Name)}", new ImportExp.NamedImport(import.Name, true)));
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
        var paramList = action.Parameters.Select(p =>
            new TsParameter(p.Name, p.Type));

        var options = new List<DictionaryEntry>();
        AddMethod(options, action.Kind);
        AddBody(options, action);
        AddQueryParams(options, action);
        AddHeaders(options, action);

        if (action.Parameters.Any(p => p.Kind == ActionParameterKind.Form))
        {
            options.Add(new DictionaryEntry(TsExp.Literal("binding"), TsExp.Literal("form")));
        }

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

    private static TsExp QueryParamExpression(ActionParameter g)
    {
        // [FromQuery] is a object
        // QueryParams: {...request};
        if (g.Type is TsInterface)
        {
            return TsExp.Spread(TsExp.Literal(g.Name));
        }
        // queryParams: { name: name };
        return TsExp.Literal(g.Name);
    }

    private static void AddQueryParams(List<DictionaryEntry> options, ActionInfo action)
    {
        // handle [FromQuery}
        var queryParams = action.Parameters
            .Where(p => p.Kind == ActionParameterKind.Query)
            .Select(g => new DictionaryEntry(QueryParamExpression(g)))
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
            var createImport = TsGenArguments.GenerateFactories && import.IsInterface  
                ? new [] { new ImportExp.NamedImport("create" + import.Name, false) } 
                : Array.Empty<ImportExp.NamedImport>();

            tsFile.Add(TsExp.Import($"./{FormatHelper.CamelCase(import.Name)}",
                new ImportExp.NamedImport(import.Name, import.IsInterface),
                createImport
            ));
        }

        tsFile.Add(TsExp.EmptyLine());
        tsFile.Add(TsExp.DefaultExport(TsExp.Interface(@interface)));

        if (TsGenArguments.GenerateFactories)
        {
            tsFile.Add(TsExp.EmptyLine());
            tsFile.Add(TsExp.NamedExport(TsExp.FunctionDef("create" + @interface.Name, @interface, ArraySegment<TsParameter>.Empty,
                BuildFactoryMethod(@interface)
            )));
        }

        tsFile.WriteToFile(Path.Combine(outputPath, TsGenArguments.InterfacesFolder, FormatHelper.CamelCase(@interface.Name)));
    }

    private static TsBlockExp BuildFactoryMethod(TsInterface @interface)
    {
        var entries = new List<DictionaryEntry>();
        // handle inheritance using spreads
        foreach (var extend in @interface.Extends)
        {
            entries.Add(new DictionaryEntry(TsExp.Spread(TsExp.FunctionCall(TsExp.Literal("create" + extend.Name), null))));
        }

        entries.AddRange(@interface.Members.Select(member => new DictionaryEntry(
            TsExp.Literal(FormatHelper.CamelCase(member.Name)), CreateType(member.Type)))
        );

        return TsExp.Block(
            TsExp.Return(
                TsExp.Dictionary(entries)
            )
        );
    }
    private static TsExp CreateType(TsType type)
    {
        switch (type)
        {
            case TsArray tsArray:
                return TsExp.Array(Array.Empty<TsExp>());
            case TsBuildInType tsBuildInType:
                return TsExp.Literal("new File([\"\"], \"empty.txt\", { type: \"text/plain\" });");
            case TsIntersection tsIntersection:
                // only if all intersection type are interfaces
                var entries = tsIntersection.Types.OfType<TsInterface>()
                    .Select(t => new DictionaryEntry(TsExp.Spread(TsExp.FunctionCall(TsExp.Literal("create" + t.Name), null))));
                return TsExp.Dictionary(entries);
            case TsTuple tsTuple:
                return TsExp.Array(tsTuple.Types.Select(CreateType));
            case TsUnion tsUnion:
                var prims = tsUnion.Types
                    .OfType<TsPrimitive>()
                    .Select(p => p.Type)
                    .ToList();

                if (prims.Contains(TsPrimitive.TsPrimitiveType.Null))
                {
                    return TsExp.Literal("null");
                }
                if (prims.Exists(t => t == TsPrimitive.TsPrimitiveType.Any || t == TsPrimitive.TsPrimitiveType.Undefined || t == TsPrimitive.TsPrimitiveType.Unknown))
                {
                    return TsExp.Literal("undefined");
                }
                // otherwise take the first one
                return CreateType(tsUnion.Types.First());
                break;
            case TsDictionary tsDictionary:
                if (tsDictionary.Key is TsEnum @enum)
                {
                    // generate keys for all @enums
                    var members = @enum.Values.Keys.Select(k => new DictionaryEntry(TsExp.Literal(k), CreateType(tsDictionary.Value)));
                    return TsExp.Dictionary(members);
                }

                return TsExp.Literal("{}");
            case TsEnum tsEnum:
                return TsExp.ObjectAccess(TsExp.Literal(tsEnum.Name), tsEnum.Values.Keys.First());
            case TsGenericReference tsGenericReference:
                return CreateType(tsGenericReference.ReferencedType);
            case TsInterface @interface:
                return TsExp.FunctionCall(TsExp.Literal("create" + @interface.Name), null);
            case TsPrimitive tsPrimitive:
                switch (tsPrimitive.Type)
                {
                    case TsPrimitive.TsPrimitiveType.Null:
                        return TsExp.Literal("null");
                    case TsPrimitive.TsPrimitiveType.String:
                        return TsExp.String("");
                    case TsPrimitive.TsPrimitiveType.Number:
                        return TsExp.Literal("0");
                    case TsPrimitive.TsPrimitiveType.Boolean:
                        return TsExp.Literal("false");
                    case TsPrimitive.TsPrimitiveType.Date:
                        return TsExp.Literal("new Date()");
                    case TsPrimitive.TsPrimitiveType.Undefined:
                    case TsPrimitive.TsPrimitiveType.Unknown:
                    case TsPrimitive.TsPrimitiveType.Any:
                    default:
                        return TsExp.Literal("undefined");
                }
        }
        return TsExp.Literal("null!");
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
