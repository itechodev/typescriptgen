using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Mvc;

namespace TypescriptGen
{
    class Program
    {
        public class RenderLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver _resolver;

            public RenderLoadContext(string location)
            {
                _resolver = new AssemblyDependencyResolver(location);
                
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                var path = _resolver.ResolveAssemblyToPath(assemblyName);
                if (path != null)
                {
                    return LoadFromAssemblyPath(path);
                }
                return null;
                // return base.Load(assemblyName);
            }
            
            protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
            {
                var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
                if (path != null)
                {
                    return LoadUnmanagedDllFromPath(path);
                }
                return IntPtr.Zero;
                // return base.LoadUnmanagedDll(unmanagedDllName);
            }
        }
        
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments provided");
                return;
            }

            var path = args[0];
            var fullPath = Path.GetDirectoryName(path);
            
            var loadContext = new RenderLoadContext(path);
            var asm = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(path)));
            
            var controllers = asm.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t))
                .Select(t => t.Name)
                .ToList();
            
            File.WriteAllText(Path.Combine(fullPath, "controllers.txt"), String.Join(", ", controllers));
            
            Console.WriteLine("Found: " + String.Join(", ", controllers));
        }
    }
}
