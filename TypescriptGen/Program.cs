using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace TypescriptGen
{
    class Program
    {
        public record TypeImport(string DefaultImport, string Filename);
        
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                // Console.WriteLine("No arguments provided");
                // return;
                args = new string[]
                {
                    "/Users/willem/projects/typescriptgen/webapi/bin/Debug/net6.0/webapi.dll",
                    "/Users/willem/projects/typescriptgen/",
                };
            }

            var path = args[0];
            var fullPath = Path.GetDirectoryName(path);
            
            var loadContext = new RenderLoadContext(path);
            var asm = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(path)));
            
            var controllers = asm
                .GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) || typeof(ControllerBase).IsAssignableFrom(t))
                .Where(t => !ControllerDiscovery.CheckIfControllerHasIgnoreAttribute(t))
                .Select(ControllerDiscovery.PopulateController)
                .ToList();
            
              var converter = new TsConverter();

            foreach (var controller in controllers)
            {
                // remove all actions that return nothing ie IActionResult
                // generate only modules that return explicitly
                controller.Actions.RemoveAll(a => a.ReturnType == typeof(IActionResult));

                if (controller.Actions.Count == 0)
                    continue;

                var resolvedTypes = controller.Actions.Select(action => new
                {
                    Action = action,
                    ReturnType = converter.ConvertType(action.ReturnType),
                    Parameters = action.Parameters.Select(p => new
                    {
                        Parameter = p,
                        TsType = converter.ConvertType(p.Type)
                    })
                }).ToList();

                var code = new CodeGenerator();
                code.DisableEsLint();
                code.Signature();
                code.WriteLine("import Axios, {type AxiosResponse} from 'axios';");
                code.WriteLine("import {queryUrl, toFormData, defaultConfig, defaultFormConfig, contentOnly} from \"./helper\";");

                var externalTypes = resolvedTypes
                    .SelectMany(r =>
                    {
                        var list = new List<TypeImport>();
                        ResolveAllTypes(list, r.ReturnType);
                        foreach (var p in r.Parameters)
                            ResolveAllTypes(list, p.TsType);

                        return list;
                    })
                    .ToList();

                ImportTypes(externalTypes, code);

                // export only for get methods
                var getMethods = resolvedTypes.Where(r => r.Action.Kind == ControllerDiscovery.ActionKind.Get).ToArray();
                if (getMethods.Any())
                {
                    code.WriteLine(null);
                    code.WriteLine("const urls = {");
                    code.Indent();
                    foreach (var getMethod in getMethods)
                    {
                        var paramsList = string.Join(", ",
                            getMethod.Parameters.Select(p =>
                                $"{TsGenerator.CamelCase(p.Parameter.Name)}: {TsGenerator.ToDefString(p.TsType)}"));

                        code.WriteLine($"{TsGenerator.CamelCase(getMethod.Action.Name)}({paramsList}): string " + "{");
                        code.Indent();
                        code.WriteLine($"return {BuildUrl(controller, getMethod.Action)};");
                        code.Outdent();
                        code.WriteLine("}" + (getMethod != getMethods.Last() ? "," : ""));
                    }
                    code.Outdent();
                    code.WriteLine("}");
                }

                code.WriteLine(null);
                code.WriteLine("export default {");
                code.Indent();
                if (getMethods.Any())
                {
                    code.WriteLine("urls,");
                }

                foreach (var type in resolvedTypes)
                {
                    var paramsList = string.Join(", ",
                        type.Parameters.Select(p =>
                            $"{TsGenerator.CamelCase(p.Parameter.Name)}: {TsGenerator.ToDefString(p.TsType)}"));

                    code.WriteLine(
                        $"{TsGenerator.CamelCase(type.Action.Name)}({paramsList}): Promise<{TsGenerator.ToDefString(type.ReturnType)}>" +
                        " {");
                    code.Indent();

                    code.WriteLine(BuildBody(controller, type.Action));
                    code.Outdent();
                    code.WriteLine("}" + (type.Action != controller.Actions.Last() ? "," : ""));
                }

                code.Outdent();
                code.WriteLine("}");

                code.Flush(Path.Combine(args[1], controller.Name + ".ts"));
            }

            // now generate all the interfaces
            GenerateInterfaces(args[1], converter.Interfaces);
            GenerateEnums(args[1], converter.Enums);

            Console.WriteLine($"{converter.Interfaces.Count} interfaces and {converter.Enums.Count} enums generated.");
        }

        private static void ImportTypes(List<TypeImport> externalTypes, CodeGenerator code)
        {
            foreach (var external in externalTypes.Distinct())
            {
                code.WriteLine($"import {@external.DefaultImport} from \"./{external.Filename}\";");
            }
        }

        private static void ResolveAllTypes(List<TypeImport> types, TsType type)
        {
            switch (type)
            {
                case TsArray tsArray:
                    ResolveAllTypes(types, tsArray.ElementType);
                    break;
                case TsDictionary tsDictionary:
                    ResolveAllTypes(types, tsDictionary.Value);
                    break;
                case TsEnum tsEnum:
                    types.Add(new TypeImport(tsEnum.Name, tsEnum.Name));
                    break;
                case TsGenericReference tsGenericReference:
                    types.Add(new TypeImport(tsGenericReference.BaseType.Name, tsGenericReference.BaseType.Name));
                    foreach (var g in tsGenericReference.Generics)
                        ResolveAllTypes(types, g);
                    break;
                case TsInterface tsInterface:
                    types.Add(new TypeImport(tsInterface.Name, tsInterface.Name));
                    break;
            }
        }

        private static void GenerateEnums(string path, List<TsEnum> enums)
        {
            foreach (var @enum in enums)
            {
                var code = new CodeGenerator();
                code.Signature();
                code.WriteLine(null);
                code.WriteLine($"enum {@enum.Name}" + " {");
                code.Indent();

                code.WriteLines(@enum.Options.Select(e => $"{e.Key} = \"{e.Value}\""), ",");

                code.Outdent();
                code.WriteLine("}");
                code.WriteLine(null);
                code.WriteLine($"export default {@enum.Name};");
                code.Flush(Path.Combine(path, @enum.Name + ".ts"));
            }
        }

        private static void GenerateInterfaces(string path, List<TsInterface> interfaces)
        {
            foreach (var @interface in interfaces)
            {
                var code = new CodeGenerator();
                code.Signature();

                var external = new List<TypeImport>();
                foreach (var member in @interface.Members)
                {
                    ResolveAllTypes(external, member.TsType);
                }

                if (@interface.Inherit != null)
                    ResolveAllTypes(external, @interface.Inherit);

                // avoid self-referencing imports
                var importedTypes = external.Where(t => t.DefaultImport != @interface.Name).ToList();
                ImportTypes(importedTypes, code);

                var inherited = @interface.Inherit != null
                    ? " extends " + @interface.Inherit.Name
                    : "";

                code.WriteLine(null);
                if (@interface.Generics.Any())
                {
                    var list = string.Join(" ,", @interface.Generics.Select(g => g.Name));
                    code.WriteLine($"export default interface {@interface.Name}<{list}>" + inherited + " {");
                }
                else
                {
                    code.WriteLine($"export default interface {@interface.Name}" + inherited + " {");
                }

                code.Indent();
                code.WriteLines(
                    @interface.Members.Select(mem =>
                        $"{TsGenerator.CamelCase(mem.Name)}: {TsGenerator.ToDefString(mem.TsType)}" + (mem.Nullable ? " | null" : "")), ",");

                code.Outdent();
                code.WriteLine("}");

                code.Flush( Path.Combine(path, @interface.Name + ".ts"));
            }
        }

        private static string BuildBody(ControllerDiscovery.ControllerInfo controller,
            ControllerDiscovery.ActionInfo action)
        {
            var url =  BuildUrl(controller, action);

            var form = action.Parameters.SingleOrDefault(p => p.Kind == ControllerDiscovery.ActionParameterKind.Form);
            if (form != null)
            {
                return "return contentOnly(Axios." + AxiosMethod(action.Kind) + "(" + url + ", toFormData(" + form.Name + "), defaultFormConfig));";
            }

            var body = action.Parameters.SingleOrDefault(p => p.Kind == ControllerDiscovery.ActionParameterKind.Body);
            return "return contentOnly(Axios." + AxiosMethod(action.Kind) + "(" + url + (body != null ? ", " + body.Name : "") + ", defaultConfig));";
        }


        // Takes in a route with segments, FarmController/{id}
        // And converts to a JS interpolated string
        private static string FormatRoutes(string url, ControllerDiscovery.ActionInfo action)
        {
            // extract url segments {}
            var routes = action
                .Parameters
                .Where(p => p.Kind == ControllerDiscovery.ActionParameterKind.Route)
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

        private static string BuildUrl(ControllerDiscovery.ControllerInfo controller,
            ControllerDiscovery.ActionInfo action)
        {
            var queryParams = FormatQueryParams(action.Parameters);
            var url = GetUrl(controller, action);
            var interpolated = FormatRoutes(url.ToLower(), action);

            if (queryParams != null)
                return "queryUrl(\"" + interpolated + "\", " + queryParams + ")";

            return "\"" + interpolated + "\"";
        }


        private static string GetUrl(ControllerDiscovery.ControllerInfo controller,
            ControllerDiscovery.ActionInfo action)
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


        private static string FormatQueryParams(List<ControllerDiscovery.ActionParameter> parameters)
        {
            var q = parameters.Where(p => p.Kind == ControllerDiscovery.ActionParameterKind.Query).ToList();
            if (!q.Any())
                return null;

            // Check is params in [FromBody] is a Object, but not an array
            if (q.Count == 1 && Type.GetTypeCode(q[0].Type) == TypeCode.Object && !q[0].Type.IsArray())
            {
                return q[0].Name;
            }
            
            return "{" + string.Join(", ", q.Select(a => a.Name)) + "}";
        }

        private static string AxiosMethod(ControllerDiscovery.ActionKind kind)
        {
            switch (kind)
            {
                case ControllerDiscovery.ActionKind.Post:
                    return "post";
                case ControllerDiscovery.ActionKind.Get:
                    return "get";
                case ControllerDiscovery.ActionKind.Patch:
                    return "patch";
                case ControllerDiscovery.ActionKind.Delete:
                    return "delete";
                case ControllerDiscovery.ActionKind.Put:
                    return "put";
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static string FormatControllerName(string name)
        {
            if (name.EndsWith("controller", StringComparison.OrdinalIgnoreCase))
                return name.Substring(0, name.Length - 10);

            return name;
        }
    }
}
