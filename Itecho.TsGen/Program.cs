using System.Reflection;

namespace Itecho.TsGen;

// dotnet pack
// dotnet tool install --global --add-source ./nupkg inspector  

public class PaginationResponse<T>
{
    public T[] Data { get; set; }
    public int Total { get; set; }
}

public class Dog
{
    public string Name { get; set; }
    public string Breed { get; set; }
}

public class SomeResponse
{
    public bool Error { get; set; }
    public PaginationResponse<Dog> Dogs { get; set; }
}

public static class Program
{
    static void Main(string[] args)
    {
        var tsType = TsConverter.Convert(typeof(SomeResponse));

        return;

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

        Console.WriteLine(string.Join("\n", controllers.Select(t => t.Name)));
    }
}