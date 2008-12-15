using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Caching;
using IronRuby;
using IronRuby.Builtins;
using Microsoft.Scripting.Hosting;
using Monarch.ActionPack.Helpers;
using Monarch.ActiveSupport;

namespace Monarch.ActionPack
{
    class View
    {
        #region Constants

        private const string modelBaseExtension = @"require 'Monarch, Version=1.0.0.0, Culture=neutral'
            class Monarch::ActiveRecord::ModelBase
                def [](key)
                    self.get_Item(key)
                end

                def method_missing(method, *args)
                    return self.get_Item(Monarch::ActiveSupport::Inflector.Pascalize(method.to_s))
                end
            end";

        #endregion

        #region Readonly & Static Fields

        private readonly string applicationPath;
        private readonly string controller;

        private static ScriptEngine engine;
        private readonly string layout;
        private readonly string view;

        #endregion

        #region Constructors

        public View(string applicationPath, string controller, string view, string layout)
        {
            this.applicationPath = applicationPath;
            this.controller = controller;
            this.view = view;
            this.layout = layout;

            SetupEngine();
        }

        #endregion

        #region Instance Methods

        public string Run(IDictionary<string, object> context)
        {
            var cacheKey = controller + view + layout;

            var scriptSource = HttpContext.Current.Cache[cacheKey] as ScriptSource;

            if (null == scriptSource)
            {
                var s = Path.DirectorySeparatorChar;

                var viewPath = Path.Combine(applicationPath, string.Format("Views{0}{1}{0}{2}.rhtml", s, controller, view));
                var layoutPath = Path.Combine(applicationPath, string.Format("Views{0}Layouts{0}{1}.rhtml", s, layout));

                var viewText = File.ReadAllText(viewPath);
                var layoutText = File.ReadAllText(layoutPath);

                var source = string.Format("{0}\n{1}; {2}", modelBaseExtension, CompileErb(viewText, "view_output"), CompileErb(layoutText, "layout_output"));

                scriptSource = engine.CreateScriptSourceFromString(source);

                scriptSource.Compile();

                HttpContext.Current.Cache.Insert(cacheKey, scriptSource, new CacheDependency(new[] {viewPath, layoutPath}));
            }

            var scope = engine.CreateScope();

            foreach (var key in context.Keys)
            {
                scope.SetVariable(Inflector.Underscore(key), context[key]);
            }

            // Add user defined helpers
            foreach (var helper in Helper.GetUserDefinedHelpers())
                scope.SetVariable(Inflector.Underscore(helper.Name), helper);

            return scriptSource.Execute<MutableString>(scope).ToString(); ;
        }

        #endregion

        #region Class Methods

        private static string CompileErb(string source, string variableName)
        {
            var scope = engine.CreateScope();

            scope.SetVariable("source", source);
            scope.SetVariable("variable_name", variableName);

            return engine.Execute<MutableString>("ERB.new(source.to_s, nil, nil, variable_name).src", scope).ToString();
        }

        private static void SetupEngine()
        {
            if (null == engine)
            {
                engine = Ruby.GetEngine(Ruby.CreateRuntime());
                engine.SetSearchPaths(Configuration.RubySearchPath);
                engine.RequireRubyFile("erb");
            }
        }

        #endregion
    }
}
