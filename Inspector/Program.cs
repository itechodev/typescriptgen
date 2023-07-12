using System.Reflection;


// Loading assemblies and dependencies manually can be quite complex having only the .csproj file as a reference.
// As an simple alternative, choose the single output folder where all dependencies are copied to load the assembly from there. 
// Assembly.LoadFile resolves dependencies automatically by probing the Global Assembly Cache (GAC) and the folder from where the assembly was loaded for the necessary dependencies.

var assembly = Assembly.LoadFrom("/Users/willem/projects/itest-leaf/Web/bin/Debug/net6.0/Database.dll");

// Now you can get types, create instances, etc.
// Type[] types = assembly.GetTypes();


Console.Write(string.Join(',', assembly.GetTypes().Select(t => t.Name)));