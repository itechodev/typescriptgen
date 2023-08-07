using System.Reflection;
using Itecho.TsGen.TsTypes;
using Microsoft.AspNetCore.Mvc;

namespace Itecho.TsGen;

public class ControllerInfo
{
    public string Name { get; set; } = string.Empty;
    public string RouteTemplate { get; set; } = string.Empty;
    public List<ActionInfo> Actions { get; set; } = new();

    public List<string> GetReferencedTypes()
    {
        var resolver = new TsImportTypeResolver(null);

        foreach (var action in Actions)
        {
            resolver.Visit(action.ReturnType);
            resolver.Visit(action.Parameters.Select(p => p.Type));
        }

        return resolver.GetImports();
    }
}

public enum ActionKind
{
    Post,
    Get,
    Patch,
    Delete,
    Put
}

public class ActionInfo
{
    public string RouteTemplate { get; set; } = string.Empty;
    public TsType ReturnType { get; set; } = TsType.Primitive(TsPrimitive.TsPrimitiveType.Unknown);
    public Type ReturnTypeClr { get; set; } = typeof(object);
    public ActionKind Kind { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ActionParameter> Parameters { get; set; } = new();
}

public enum ActionParameterKind
{
    Route,
    Body,
    Query,
    Form,
    Header
}

public class ActionParameter
{
    public string Name { get; set; }
    public ActionParameterKind Kind { get; set; }
    public object? DefaultValue { get; set; }
    public TsType Type { get; set; }

    public ActionParameter(string name, ActionParameterKind kind, object? defaultValue, TsType type)
    {
        Name = name;
        Kind = kind;
        DefaultValue = defaultValue;
        Type = type;
    }
}

public static class ControllerInspector
{
    private static ActionKind PopulateMethodKind(MethodInfo methodInfo)
    {
        if (methodInfo.GetCustomAttribute<HttpPostAttribute>() != null)
            return ActionKind.Post;

        if (methodInfo.GetCustomAttribute<HttpDeleteAttribute>() != null)
            return ActionKind.Delete;

        if (methodInfo.GetCustomAttribute<HttpPatchAttribute>() != null)
            return ActionKind.Patch;

        if (methodInfo.GetCustomAttribute<HttpPutAttribute>() != null)
            return ActionKind.Put;

        return ActionKind.Get;
    }

    private static ActionParameterKind GetKind(ParameterInfo param)
    {
        if (param.GetCustomAttribute<FromBodyAttribute>() != null)
            return ActionParameterKind.Body;

        if (param.GetCustomAttribute<FromQueryAttribute>() != null)
            return ActionParameterKind.Query;

        if (param.GetCustomAttribute<FromFormAttribute>() != null)
            return ActionParameterKind.Form;

        if (param.GetCustomAttribute<FromHeaderAttribute>() != null)
            return ActionParameterKind.Header;

        // the default is route
        // if no binding are Route, Query, and Form
        // fallback to Query
        return ActionParameterKind.Query;
    }

    private static ActionParameter? PopulateParameter(ParameterInfo param)
    {
        // Ignore service attribute. They are automatically injected by the framework
        if (param.GetCustomAttribute<FromServicesAttribute>() != null)
            return null;

        return new ActionParameter(
            param.Name ?? "", GetKind(param),
            param.DefaultValue,
            TsConverter.Convert(param.ParameterType,
                NullableHelper.IsNullable(param))
        );
    }

    private static bool IsExplicitReturn(TsType type)
    {
        // if explicit returns is not set, then we generate all methods
        if (!TsGenArguments.ExplicitReturns)
            return true;

        return type switch
        {
            TsVoid => TsGenArguments.GenerateVoidReturn,
            TsPrimitive prim => prim.Type != TsPrimitive.TsPrimitiveType.Any &&
                                prim.Type != TsPrimitive.TsPrimitiveType.Undefined &&
                                prim.Type != TsPrimitive.TsPrimitiveType.Unknown,

            // anything else is considered explicit
            _ => true
        };
    }

    private static ActionInfo PopulateMethod(TsType returnType, MethodInfo methodInfo)
    {
        // there may exists multiple routes
        // take the first one
        var route = methodInfo.GetCustomAttributes<RouteAttribute>().FirstOrDefault();

        return new ActionInfo()
        {
            Name = methodInfo.Name,
            RouteTemplate = route?.Template ?? "",
            ReturnType = returnType,
            ReturnTypeClr = methodInfo.ReturnType,
            Kind = PopulateMethodKind(methodInfo), Parameters = methodInfo
                .GetParameters()
                .Select(PopulateParameter)
                .Where(p => p != null)
                .Cast<ActionParameter>()
                .ToList()
        };
    }


    private static ControllerInfo PopulateController(Type controller)
    {
        // multiple routes may exists, take the first one
        var route = controller.GetCustomAttributes<RouteAttribute>().FirstOrDefault();

        var actions = controller
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly |
                        BindingFlags.InvokeMethod)
            .Where(FilterMethod)
            
            // we only interested in methods that explicit returns a value
            // don't further parse the methods arguments etc
            .Select(methodInfo => new
            {
                ReturnType = TsConverter.Convert(methodInfo.ReturnType),
                MethodInfo = methodInfo
            })
            .Where(m => IsExplicitReturn(m.ReturnType))
            .Select(m => PopulateMethod(m.ReturnType, m.MethodInfo));
        
        return new ControllerInfo()
        {
            Name = controller.Name, 
            RouteTemplate = route?.Template ?? string.Empty, 
            Actions = actions
                .ToList()
        };
    }

    private static bool FilterMethod(MethodInfo m)
    {
        // GetMethods() returns all the methods of the type including property getters and setters because they are technically methods (with special names).
        // Unfortunately, there's no direct way to filter out the property methods using only binding flags.
        // Property getter methods are named with a "get_" prefix, and setter methods are named with a "set_" prefix.
        return !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_");
    }

    public static List<ControllerInfo> Inspect(Assembly assembly)
    {
        return assembly
            .GetExportedTypes()
            .Where(t => (typeof(Controller).IsAssignableFrom(t) || typeof(ControllerBase).IsAssignableFrom(t)) &&
                        !TsGenArguments.Ignore.Contains(t.Name.ToLower()))
            .Select(PopulateController)
            .ToList();
    }
}