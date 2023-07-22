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
        return "/" + (name.EndsWith("controller", StringComparison.OrdinalIgnoreCase) ? name[..^10] : name);

    }

    public static string ActionMethod(ActionKind kind)
    {
        return kind switch
        {
            ActionKind.Post => "post", ActionKind.Get => "get", ActionKind.Patch => "patch", ActionKind.Delete => "delete", ActionKind.Put => "put", _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }


    private static string GetUrl(ControllerInfo controller, ActionInfo action)
    {
        if (!string.IsNullOrEmpty(action.RouteTemplate))
        {
            if (action.RouteTemplate.StartsWith("/"))
                return action.RouteTemplate;

            // when using route attributes on both the controller and actions, the routes is combined
            return controller.RouteTemplate + action.RouteTemplate;
        }

        if (!string.IsNullOrEmpty(controller.RouteTemplate))
        {
            return controller.RouteTemplate + "/" + action.Name;
        }

        // fallback to the default route controller/action
        return FormatControllerName(controller.Name) + "/" + action.Name;
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
        var extract = new Regex(@"\{.*\}");
        var matches = extract.Matches(url);

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
        foreach (Match match in matches)
        {
            if (match.Index > prev)
            {
                segments.Add(InterpolatedStringExp.String(url.Substring(prev, match.Index)));
            }

            segments.Add(InterpolatedStringExp.Expression(TsExp.Literal(url.Substring(match.Index, match.Length))));
            prev = match.Index + match.Length;
        }

        return TsExp.InterpolatedString(segments.ToArray());
    }
}
