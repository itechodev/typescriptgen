using System.Text.RegularExpressions;
using Itecho.TsGen.TSExpressions;
using Microsoft.AspNetCore.Mvc;

namespace Itecho.TsGen;

public static class RouteHelper
{
    /// <summary>
    /// build the url for the action
    /// influenced by FromRoute parameters
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static TsExp BuildUrl(ControllerInfo controller, ActionInfo action)
    {
        // (value: number, name: string | null) => buildUrl('/home/isOdd'. {value, name});
        // (value: number, route: string) => buildUrl(`/home/isOdd/${route}`. {value, name});
        var url = GetUrl(controller, action);

        // first handle the route parameters if any.
        // convert to interpolated string or normal string
        return FormatRoutes(url.ToLower(), action);
    }

    private static string FormatControllerName(string name)
    {
        return name.EndsWith("controller", StringComparison.OrdinalIgnoreCase) ? name[..^10] : name;
    }

    public static string ActionMethod(ActionKind kind)
    {
        return kind switch
        {
            ActionKind.Post => "post", ActionKind.Get => "get", ActionKind.Patch => "patch",
            ActionKind.Delete => "delete", ActionKind.Put => "put",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    
    
    private static string RoutePlaceHolders(string route, string area, string controller, string action)
    {
        // replace common conventional placeholders
        // attribute routing, you define placeholders by using {...} syntax instead of square brackets.
        route = route.Replace("[controller]", controller);
        route = route.Replace("[action]", action);
        route = route.Replace("[area]", area);

        route = route.Replace("{controller}", controller);
        route = route.Replace("{action}", action);
        route = route.Replace("{area}", area);

        return route;
    }

    private static string GetUrl(ControllerInfo controller, ActionInfo action)
    {
        if (!string.IsNullOrEmpty(action.RouteTemplate))
        {
            if (action.RouteTemplate.StartsWith("/"))
                return RoutePlaceHolders(action.RouteTemplate, controller.Area, FormatControllerName(controller.Name), action.Name);

            // when using route attributes on both the controller and actions, the routes is combined.
            // ASP.NET joins with a single '/' regardless of trailing/leading slashes on either side.
            return RoutePlaceHolders(JoinRoutes(controller.RouteTemplate, action.RouteTemplate), controller.Area, FormatControllerName(controller.Name), action.Name);
        }

        if (!string.IsNullOrEmpty(controller.RouteTemplate))
        {
            return RoutePlaceHolders(controller.RouteTemplate, controller.Area, FormatControllerName(controller.Name), action.Name);
        }

        // fallback to the default route controller/action
        return RoutePlaceHolders(TsGenArguments.DefaultPattern, controller.Area, FormatControllerName(controller.Name), action.Name);
    }

    private static string JoinRoutes(string controllerRoute, string actionRoute)
    {
        if (string.IsNullOrEmpty(controllerRoute)) return actionRoute;
        if (string.IsNullOrEmpty(actionRoute)) return controllerRoute;
        var c = controllerRoute.TrimEnd('/');
        var a = actionRoute.TrimStart('/');
        return c + "/" + a;
    }

    
    // Matches any {...} segment in a route template. Non-greedy so two
    // adjacent placeholders (`{a}/{b}`) are picked up as separate matches.
    private static readonly Regex Extract = new(@"\{[^}]+\}");

    /// <summary>
    /// Extracts the bare parameter names from a route template, stripping
    /// the braces and any route constraint (e.g. ":guid", "?", "=default").
    /// </summary>
    public static IReadOnlyList<string> ExtractRouteParameterNames(string routeTemplate)
    {
        var names = new List<string>();
        foreach (Match m in Extract.Matches(routeTemplate))
        {
            var inner = m.Value.Trim('{', '}');
            names.Add(StripRouteConstraint(inner));
        }
        return names;
    }

    // Takes in a route with segments, FarmController/{id}
    // And converts to a JS interpolated string
    private static TsExp FormatRoutes(string url, ActionInfo action)
    {
        // extract url segments {}
        var routes = action
            .Parameters
            .Where(p => p.Kind == ActionParameterKind.Route)
            .ToArray();

        // there are no routes defined, return the url as is
        if (routes.Length == 0)
            return TsExp.String(url);

        // extract all {.} pairs from the URL
        // then replace it with route parameter
        var matches = Extract.Matches(url);

        // there are routes defined, but pair in URL does not match
        // example: [HttpGet] public Response Method(int param1). param1 is considered a route parameter, but no route defined
        // fallback to construct with /
        if (matches.Count != routes.Length)
        {
            throw new Exception(
                $"Could not build URL. There are {routes.Length} parameter routes defined with {matches.Count} url segment in action `{action.Name}`.\n Url provided: {url} with route paramters {string.Join(" ", routes.Select(r => r.Name))}");
        }

        var segments = new List<InterpolatedSegment>();
        var prev = 0;
        var routeIndex = 0;
        foreach (Match match in matches)
        {
            if (match.Index > prev)
            {
                segments.Add(InterpolatedStringExp.String(url.Substring(prev, match.Index - prev)));
            }

            // Use the parameter name from the action signature rather than the
            // raw `{id:guid}` from the route template — strips ASP.NET route
            // constraints and matches the actual TS parameter the caller passes in.
            var paramName = routeIndex < routes.Length
                ? routes[routeIndex].Name
                : StripRouteConstraint(url.Substring(match.Index + 1, match.Length - 2));
            segments.Add(InterpolatedStringExp.Expression(TsExp.Literal(paramName)));
            prev = match.Index + match.Length;
            routeIndex++;
        }

        if (prev < url.Length)
        {
            segments.Add(InterpolatedStringExp.String(url.Substring(prev)));
        }

        return TsExp.InterpolatedString(segments.ToArray());
    }

    // {id:guid} -> id, {name?} -> name, {id=5} -> id
    private static string StripRouteConstraint(string segment)
    {
        var colon = segment.IndexOf(':');
        if (colon >= 0) segment = segment.Substring(0, colon);
        var question = segment.IndexOf('?');
        if (question >= 0) segment = segment.Substring(0, question);
        var equals = segment.IndexOf('=');
        if (equals >= 0) segment = segment.Substring(0, equals);
        return segment;
    }
}