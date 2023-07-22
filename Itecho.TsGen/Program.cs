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
            TSGenerator.GenerateInterface(@interface, outputPath);
        }

        foreach (var controller in controllers)
        {
            TSGenerator.GenerateController(controller, outputPath);
        }

        // generate the general http handler
        RequestOptions.Generate().WriteToFile(Path.Combine(outputPath, "requestOptions"));
        // and the http concrete handler for the user to change
        var httpClientPath = Path.Combine(outputPath, "makeRequest");
        if (!File.Exists(httpClientPath))
        {
            HttpMakeRequestFile.Generate().WriteToFile(httpClientPath);
        }
        Console.WriteLine("Done");
    }
}
