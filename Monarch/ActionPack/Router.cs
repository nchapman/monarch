using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Monarch.ActionPack
{
    public class Router
    {
        #region Readonly & Static Fields

        private static readonly Dictionary<string, RouteMatch> matchCache = new Dictionary<string, RouteMatch>();
        private static readonly Regex paramsRegex = new Regex(@":([^/\.]+)", RegexOptions.Compiled);
        private static readonly List<Route> routes = new List<Route>();

        #endregion

        #region Class Methods

        public static void Add(string pattern)
        {
            Add(pattern, null, null);
        }

        public static void Add(string pattern, string controller, string action)
        {
            // Allows for patterns like /:controller/:action.ashx
            var parsedPattern = paramsRegex.Replace(pattern, @"(?<$1>[^/\.]+)");

            routes.Add(new Route { Action = action, Controller = controller, PatternRegex = new Regex(parsedPattern, RegexOptions.Compiled) });
        }

        internal static RouteMatch Match(string path)
        {
            var toReturn = new RouteMatch {Success = false};

            if (matchCache.ContainsKey(path))
            {
                toReturn = matchCache[path];
            }
            else
            {
                foreach (var route in routes)
                {
                    var match = route.PatternRegex.Match(path);

                    if (match.Success)
                    {
                        var controller = match.Groups["controller"].Value != "" ? match.Groups["controller"].Value : route.Controller;
                        var action = match.Groups["action"].Value != "" ? match.Groups["action"].Value : route.Action;

                        toReturn.Success = true;
                        toReturn.Controller = controller;
                        toReturn.Action = action;

                        toReturn.PathParameters = new Dictionary<string, string>();

                        foreach (var name in route.PatternRegex.GetGroupNames())
                            if (null != match.Groups[name])
                                toReturn.PathParameters.Add(name, match.Groups[name].Value);

                        matchCache.Add(path, toReturn);

                        break;
                    }
                }
            }

            return toReturn;
        }

        #endregion
    }
}
