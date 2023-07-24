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
        var resolver = new TsImportTypeResolver();

        foreach (var action in Actions)
        {
            resolver.Resolve(action.ReturnType);
            resolver.Resolve(action.Parameters.Select(p => p.Type));
        }

        return resolver.GetImports(Name);
    }
}

public enum ActionKind
{
    Post, Get, Patch, Delete, Put
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
    Route, Body, Query, Form, Header
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
        return ActionParameterKind.Route;
    }

    private static ActionParameter? PopulateParameter(ParameterInfo param)
    {
        // Ignore service attribute. They are automatically injected by the framework
        if (param.GetCustomAttribute<FromServicesAttribute>() != null)
            return null;

        return new ActionParameter(param.Name ?? "", GetKind(param), param.DefaultValue,
            TsConverter.Convert(param.ParameterType, NullableHelper.IsNullable(param)));
    }


    private static ActionInfo PopulateMethod(MethodInfo methodInfo)
    {
        var route = methodInfo.GetCustomAttribute<RouteAttribute>();

        return new ActionInfo()
        {
            Name = methodInfo.Name,
            RouteTemplate = route?.Template ?? "",
            ReturnType = TsConverter.Convert(methodInfo.ReturnType),
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
        var route = controller.GetCustomAttribute<RouteAttribute>();
        return new ControllerInfo()
        {
            Name = controller.Name, RouteTemplate = route?.Template ?? string.Empty, Actions = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Select(PopulateMethod)
                .ToList()
        };
    }

    public static List<ControllerInfo> Inspect(Assembly assembly)
    {
        return assembly
            .GetExportedTypes()
            .Where(t => (typeof(Controller).IsAssignableFrom(t) || typeof(ControllerBase).IsAssignableFrom(t)) && !TsGenArguments.Ignore.Contains(t.Name.ToLower()))
            .Select(PopulateController)
                .ToList();
    }

}
