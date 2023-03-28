using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace TypescriptGen;

public class IgnoreTypeGenAttribute : Attribute
{
}
public static class ControllerDiscovery
{
    public class ControllerInfo
    {
        public string Name { get; set; } = string.Empty;
        public string RouteTemplate { get; set; } = string.Empty;
        public List<ActionInfo> Actions { get; set; } = new();
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
        public Type? ReturnType { get; set; }
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
        public string Name { get; set; } = string.Empty;
        public ActionParameterKind Kind { get; set; }
        public object? DefaultValue { get; set; }
        public Type? Type { get; set; }
    }
    
    public static bool CheckIfControllerHasIgnoreAttribute(Type controller)
    {
        return controller.GetCustomAttributes().Any(a => a.GetType().Name == nameof(IgnoreTypeGenAttribute));
    }

    public static ControllerInfo PopulateController(Type controller)
    {
        var route = controller.GetCustomAttribute<RouteAttribute>();
        return new ControllerInfo()
        {
            Name = controller.Name,
            RouteTemplate = route?.Template ?? string.Empty,
            Actions = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Select(PopulateMethod)
                .ToList()
        };
    }

    private static ActionInfo PopulateMethod(MethodInfo methodInfo)
    {
        var route = methodInfo.GetCustomAttribute<RouteAttribute>();

        return new ActionInfo()
        {
            Name = methodInfo.Name,
            RouteTemplate = route?.Template ?? "",
            ReturnType = methodInfo.ReturnType,
            Kind = PopulateMethodKind(methodInfo),
            Parameters = methodInfo
                .GetParameters()
                .Select(PopulateParameter)
                .Where(p => p != null)
                .Cast<ActionParameter>()
                .ToList()
        };
    }

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

    private static ActionParameter? PopulateParameter(ParameterInfo param)
    {
        // Ignore service attribute
        if (param.GetCustomAttribute<FromServicesAttribute>() != null)
            return null;

        return new ActionParameter()
        {
            Name = param.Name ?? "",
            DefaultValue = param.DefaultValue,
            Kind = GetKind(param),
            Type = param.ParameterType
        };
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
}