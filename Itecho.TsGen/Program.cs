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
        // var urls = controller
        //     .Actions
        //     .Where(r => r.Kind == ActionKind.Get)
        //     .Select(g => new DictionaryEntry(TsExp.Literal(g.Name),
        //         RouteHelper.BuildUrl(controller, g)));
        //
        // tsFile.Add(TsExp.Assign(
        //     TsExp.VariableDef("urls", VariableType.Const, null),
        //     TsExp.Dictionary(urls)
        // ));

        var exportEntries = new List<DictionaryEntry>
        {
            // new(TsExp.Literal("urls"))
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
        /*
         httpClient<T>(url: string, options?: HttpOptions): Promise<T>
         export interface HttpOptions {
            method?: 'get' | 'post' | 'put' | 'patch' | 'delete';
            body?: object | FormData;
            queryParams?: Record<string, unknown>;
            headers?: Record<string, unknown>;
        }
        */

        var returnType = TsType.GenericReference(TsType.BuildIn("Promise"), action.ReturnType);

        var paramList = action.Parameters.Select(p =>
            new TsParameter(p.Name, p.Type));

        var options = new List<DictionaryEntry>();
        // { method: 'get'} is the default
        // for anything else we need to explicit pass it
        if (action.Kind != ActionKind.Get)
        {
            options.Add(new DictionaryEntry(TsExp.Literal("method"), TsExp.String(action.Kind.ToString().ToLower())));
        }

        // handle [FromBody] and [FromForm]
        var bodyParam = action.Parameters.SingleOrDefault(p => p.Kind is ActionParameterKind.Body or ActionParameterKind.Form);
        if (bodyParam != null)
        {
            options.Add(new DictionaryEntry(TsExp.Literal("body"), TsExp.Literal(bodyParam.Name)));
        }

        // handle [FromQuery}
        var queryParams = action.Parameters
            .Where(p => p.Kind == ActionParameterKind.Query)
            .Select(g => new DictionaryEntry(TsExp.Literal(g.Name)))
            .ToList();
        if (queryParams.Any())
        {
            options.Add(new DictionaryEntry(TsExp.Literal("queryParams"), TsExp.Dictionary(queryParams)));
        }

        var clientParams = new List<TsExp>()
        {
            RouteHelper.BuildUrl(controller, action)
        };
        if (options.Any())
        {
            clientParams.Add(TsExp.Dictionary(options));
        }

        return TsExp.Lambda(returnType, paramList, TsExp.Block(
            TsExp.Return(
                TsExp.FunctionCall(TsExp.Literal("httpClient"), clientParams.ToArray()
                )
            )
        ));
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
