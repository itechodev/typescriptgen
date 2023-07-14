using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;


// Loading assemblies and dependencies manually can be quite complex having only the .csproj file as a reference.
// As an simple alternative, choose the single output folder where all dependencies are copied to load the assembly from there. 
// Assembly.LoadFile resolves dependencies automatically by probing the Global Assembly Cache (GAC) and the folder from where the assembly was loaded for the necessary dependencies.

var assemblyPath = "/Users/willem/projects/itest-leaf/Web/bin/Debug/net6.0/Web.dll";

// var loadContext = new DependencyLoadContext(assemblyPath);
// var assembly = loadContext.LoadFromAssemblyName(new AssemblyName("Web"));
//
//
// var types = assembly.GetTypes().Select(t => t.Name).ToList();
// Console.WriteLine(string.Join(", ", types));


// ...

static bool IsController(MetadataReader reader, TypeDefinition type)
{
    while (true)
    {
        if (reader.GetString(type.Name) == "Controller")
        {
            return true;
        }

        if (type.BaseType.IsNil) return false;

        switch (type.BaseType.Kind)
        {
            // Check if BaseType is a TypeReference
            case HandleKind.TypeReference:
            {
                var baseType = reader.GetTypeReference((TypeReferenceHandle)type.BaseType);
                var baseTypeName = reader.GetString(baseType.Name);

                // Adjust the condition based on your specific requirements
                if (baseTypeName == "Controller" || baseTypeName.EndsWith("Controller"))
                {
                    return true;
                }

                break;
            }
            case HandleKind.TypeDefinition:
            {
                var baseType = reader.GetTypeDefinition((TypeDefinitionHandle)type.BaseType);
                type = baseType;
                continue;
            }
        }

        return false;
    }
}

static string GetTypeDisplayName(MetadataReader reader, EntityHandle handle)
{
    switch (handle.Kind)
    {
        case HandleKind.TypeDefinition:
            var typeDef = reader.GetTypeDefinition((TypeDefinitionHandle)handle);
            return reader.GetString(typeDef.Name);

        case HandleKind.TypeReference:
            var typeRef = reader.GetTypeReference((TypeReferenceHandle)handle);
            return reader.GetString(typeRef.Name);

        // Add more case labels here if you need to handle more kinds of types

        default:
            return "<unknown>";
    }
}


using (var stream = File.OpenRead(assemblyPath))
{
    using (var peReader = new PEReader(stream))
    {
        var metadataReader = peReader.GetMetadataReader();

        foreach (var typeDefinitionHandle in metadataReader.TypeDefinitions)
        {
            var typeDefinition = metadataReader.GetTypeDefinition(typeDefinitionHandle);
            var typeName = metadataReader.GetString(typeDefinition.Name);

            if (IsController(metadataReader, typeDefinition))
            {
                Console.WriteLine($"Controller found: {typeName}");


                foreach (var methodHandle in typeDefinition.GetMethods())
                {
                    var method = metadataReader.GetMethodDefinition(methodHandle);

                    // Check if the method is public
                    if ((method.Attributes & MethodAttributes.Public) == MethodAttributes.Public)
                    {
                        var methodName = metadataReader.GetString(method.Name);
                        
                        var returnType = method.Signature;
                        var returnTypeName = GetTypeDisplayName(metadataReader, returnType);
                        Console.WriteLine($"{returnTypeName} {methodName}()");
                        
                        Console.WriteLine($"  methodName: {methodName}");
                    }
                }
            }
        }
    }
}