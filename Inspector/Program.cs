using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace inspector;

// dotnet pack
//  dotnet tool install --global --add-source ./nupkg inspector  

internal class Program
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

        Console.WriteLine("Nice");
    }
}
//
// // Loading assemblies and dependencies manually can be quite complex having only the .csproj file as a reference.
// // As an simple alternative, choose the single output folder where all dependencies are copied to load the assembly from there. 
// // Assembly.LoadFile resolves dependencies automatically by probing the Global Assembly Cache (GAC) and the folder from where the assembly was loaded for the necessary dependencies.
//
// var assemblyPath = "/Users/willem/projects/itest-leaf/Web/bin/Debug/net6.0/Web.dll";
//
// var loadContext = new DependencyLoadContext(assemblyPath);
// var assembly = loadContext.LoadFromAssemblyName(new AssemblyName("Web"));
//
// // var assembly = Assembly.LoadFile(assemblyPath);
//
// foreach (var type in assembly.GetExportedTypes())
// {
//     if (type.Name == "AuthController")
//     {
//         
//         type.GetMethods().Select(m => m.Name).ToList().ForEach(Console.WriteLine);
//     }
//     // Console.WriteLine($"{type.Name} {type.FullName}");
// }
//
// // var types = assembly.GetTypes().Select(t => t.Name).ToList();