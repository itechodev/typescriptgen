using Itecho.TsGen.TSExpressions;
using Itecho.TsGen.TsTypes;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace Itecho.TsGen;

// dotnet pack
// dotnet tool install --global --add-source ./nupkg inspector  

public static class Program
{

    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine($"typescript generator v{VersionInfo.VersionString}");
            Console.WriteLine("-------------");
            Console.WriteLine("\nUsage:");
            return;
        }

        var assemblyPath = Path.GetFullPath(args[0]);
        var outputPath = Path.GetFullPath(args[1]);
        Console.WriteLine("Inspecting assembly: " + assemblyPath);
        Console.WriteLine("Output path: " + outputPath);

        // Loading assembly dependencies from a complete output path
        // using a custom AssemblyLoadContext and AssemblyDependencyResolver
        var loadContext = new DependencyLoadContext(assemblyPath);
        var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);

        var controllers = ControllerInspector.Inspect(assembly);

        var interfaces = TsConverter.Cache.Values
            .OfType<TsInterface>()
            .ToList();

        // generate interfaces per file
        foreach (var @interface in interfaces)
        {
            GenerateInterface(@interface, outputPath);
        }

        foreach (var controller in controllers)
        {
            GenerateController(controller, outputPath);
        }

        // generate the general http handler
        HttpOptions.Generate().WriteToFile(Path.Combine(outputPath, "httpOptions"));
        // and the http concrete handler for the user to change
        var httpClientPath = Path.Combine(outputPath, "httpClient");
        if (!File.Exists(httpClientPath))
        {
            HttpClientFile.Generate().WriteToFile(httpClientPath);
        }
        Console.WriteLine("Done");
    }


    private static void GenerateController(ControllerInfo controller, string outputPath)
    {
        var tsFile = new TsFile();
        tsFile.Add(TsExp.Comment("eslint-disable", true));
        tsFile.Add(VersionInfo.GenerationNotice);

        tsFile.Add(TsExp.EmptyLine());
        // import the user customisable http client 
        tsFile.Add(TsExp.Import("./httpClient", new ImportExp.NamedImport("httpClient", false)));
        tsFile.Add(TsExp.EmptyLine());

        // import all references for this controller
        foreach (var import in controller.GetReferencedTypes())
        {
            tsFile.Add(TsExp.Import($"./{import}", new ImportExp.NamedImport(import, true)));
            tsFile.Add(TsExp.EmptyLine());
        }

        tsFile.Add(TsExp.EmptyLine());
        // export only for get methods
        // const urls = {
        //     index(p1: number, p2: number): string {
        //         return "/api/address/index";
        //     }
        // }
        var urls = controller
            .Actions
            .Where(r => r.Kind == ActionKind.Get)
            .Select(g => new DictionaryEntry(TsExp.Literal(g.Name),
                RouteHelper.BuildUrl(controller, g)));

        tsFile.Add(TsExp.Assign(
            TsExp.VariableDef("urls", VariableType.Const, null),
            TsExp.Dictionary(urls)
        ));

        var exportEntries = new List<DictionaryEntry>
        {
            new(TsExp.Literal("urls"))
        };

        exportEntries.AddRange(controller.Actions.Select(action => new DictionaryEntry(
            TsExp.Literal(action.Name),
            BuildControllerAction(controller, action)))
        );

        tsFile.Add(TsExp.DefaultExport(TsExp.Dictionary(exportEntries)));

        tsFile.WriteToFile(Path.Combine(outputPath,
            FormatHelper.CamelCase(controller.Name)));
    }

    private static TsExp BuildControllerAction(ControllerInfo controller, ActionInfo action)
    {
        // should conform the httpHandler.ts
        // <TReq, TRes>(url: string, params?: HttpDic, body?: FormData | TReq, headers?: HttpDic) => Promise<TRes>;

        // form, body, query, route, header bindings.

        //     upsert(request: AddressRequest): Promise<AxiosResponse> {
        //         return httpClient.post("/api/address/upsert", undefined, request, undefined);
        //     },

        var returnType = TsType.GenericReference(TsType.BuildIn("Promise"), action.ReturnType);

        var paramList = action.Parameters.Select(p =>
            new TsParameter(p.Name, p.Type));

        // use route parameters to build the url
        // all the rest are used inside the call

        var urlsParams = action.Parameters
            .Where(a => a.Kind is (ActionParameterKind.Query or ActionParameterKind.Route))
            .Select(p => TsExp.Literal(p.Name))
            .ToArray();


        return TsExp.Lambda(returnType, paramList,
            TsExp.Block(
                TsExp.Return(
                    TsExp.FunctionCall(
                        TsExp.ObjectAccess(TsExp.Literal("http"),
                            RouteHelper.ActionMethod(action.Kind)
                        ),
                        // urls.name(param1, param2)
                        TsExp.FunctionCall(
                            TsExp.ObjectAccess(TsExp.Literal("urls"), action.Name),
                            urlsParams
                        )
                    )
                )
            )
        );
    }

    private static void GenerateInterface(TsInterface @interface, string outputPath)
    {
        var tsFile = new TsFile();
        tsFile.Add(VersionInfo.GenerationNotice);

        // import all interfaces referenced by this interface
        foreach (var import in @interface.GetReferencedTypes())
        {
            tsFile.Add(TsExp.Import($"./{import}", new ImportExp.NamedImport(import, true)));
        }

        tsFile.Add(TsExp.EmptyLine());
        tsFile.Add(TsExp.DefaultExport(TsExp.Interface(@interface)));

        tsFile.WriteToFile(Path.Combine(outputPath, FormatHelper.CamelCase(@interface.Name)));
    }
}
