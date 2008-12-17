using System.IO;
using System.Web;
using System.Web.Caching;

namespace Monarch.ActionPack
{
    class View
    {
        #region Readonly & Static Fields

        private readonly string applicationPath;
        private readonly string controller;

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
        }

        #endregion

        #region Instance Methods

        public string Run(ViewDictionary data)
        {
            var cacheKey = controller + view + layout;

            var template = HttpContext.Current.Cache[cacheKey] as BoostTemplate;

            if (null == template)
            {
                var s = Path.DirectorySeparatorChar;

                var viewPath = Path.Combine(applicationPath, string.Format("Views{0}{1}{0}{2}.rhtml", s, controller, view));
                var layoutPath = Path.Combine(applicationPath, string.Format("Views{0}Layouts{0}{1}.rhtml", s, layout));

                var viewText = File.ReadAllText(viewPath);
                var layoutText = File.ReadAllText(layoutPath);

                template = new BoostTemplate(viewText, layoutText);

                HttpContext.Current.Cache.Insert(cacheKey, template, new CacheDependency(new[] { viewPath, layoutPath }));
            }

            return template.Run(data);
        }

        #endregion
    }
}
