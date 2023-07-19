using System.Reflection;
using Itecho.TsGen.Ts.Types;

namespace Itecho.TsGen;

// dotnet pack
// dotnet tool install --global --add-source ./nupkg inspector  

public static class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            var versionString = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            Console.WriteLine($"typescript generator v{versionString}");
            Console.WriteLine("-------------");
            Console.WriteLine("\nUsage:");
            return;
        }

        var assemblyPath = args[0];
        Console.WriteLine("Inspecting assembly: " + assemblyPath);

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
            var tsCodeGenerator = new TsCodeGenerator();
            tsCodeGenerator.WriteLine("export interface " + @interface.Name);
            tsCodeGenerator.Block(g =>
            {
                foreach (var property in @interface.Members)
                {
                    g.WriteLine(property.Name + ": " + TsTypeGenerator.Generate(property.Type));
                }
            });

            var fileName = @interface.Name + ".ts";
            tsCodeGenerator.WriteToFile(fileName);
        }
    }
}