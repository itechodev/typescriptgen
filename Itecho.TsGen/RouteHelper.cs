using System.Text.RegularExpressions;

namespace Itecho.TsGen;

public static class RouteHelper
{
    private static string FormatQueryParams(List<ActionParameter> parameters)
    {
        var q = parameters.Where(p => p.Kind == ActionParameterKind.Query).ToList();
        if (!q.Any())
            return null;

        // Check is params in [FromBody] is a Object, but not an array
        // if (q.Count == 1 && Type.GetTypeCode(q[0].Type) == TypeCode.Object && !q[0].Type.IsArray())
        // {
        //     return q[0].Name;
        // }
        //     
        return "{" + string.Join(", ", q.Select(a => a.Name)) + "}";
    }

    private static string FormatControllerName(string name)
    {
        if (name.EndsWith("controller", StringComparison.OrdinalIgnoreCase))
            return name.Substring(0, name.Length - 10);

        return name;
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
        return "/api/" + FormatControllerName(controller.Name) + "/" + action.Name;
    }


    public static string BuildUrl
    (
        ControllerInfo controller,
        ActionInfo action
    )
    {
        var queryParams = FormatQueryParams(action.Parameters);
        var url = GetUrl(controller, action);
        var interpolated = FormatRoutes(url.ToLower(), action);

        if (queryParams != null)
            return "queryUrl(\"" + interpolated + "\", " + queryParams + ")";

        return "\"" + interpolated + "\"";
    }

    // Takes in a route with segments, FarmController/{id}
    // And converts to a JS interpolated string
    private static string FormatRoutes(string url, ActionInfo action)
    {
        // extract url segments {}
        var routes = action
            .Parameters
            .Where(p => p.Kind == ActionParameterKind.Route)
            .ToArray();

        if (routes.Length == 0)
            return url;

        // extract all {.} pairs from the URL
        // then replace it with route parameter
        var extract = new Regex(@"\{.*\}");
        var matches = extract.Matches(url);

        // there are routes defined, but pair in URL does not match
        // example: [HttpGet] public Response Method(int param1). param1 is considered a route parameter, but no route defined
        // fallback to construct with /
        if (matches.Count != routes.Length)
        {
            throw new Exception($"Could not build URL. There are {routes.Length} parameter routes defined with {matches.Count} url segment in action `{action.Name}`.\n Url provided: {url} with route paramters {string.Join(" ", routes.Select(r => r.Name))}");
        }

        var newUrl = "";
        var prev = 0;
        foreach (Match match in matches)
        {
            newUrl += url.Substring(prev, match.Index) + "$" + url.Substring(match.Index, match.Length);
            prev = match.Index + match.Length;
        }

        return newUrl;
    }
}
