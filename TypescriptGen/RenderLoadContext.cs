using System;
using System.Reflection;
using System.Runtime.Loader;

namespace TypescriptGen;

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