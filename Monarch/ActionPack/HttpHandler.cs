using System.Diagnostics;
using System.IO;
using System.Web;

namespace Monarch.ActionPack
{
    class HttpHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public void ProcessRequest(HttpContext context)
        {
            var sw = new Stopwatch();
            
            sw.Start();

            var routeMatch = Router.Match(context.Request.Path);

            if (routeMatch.Success)
            {
                var controller = Controller.Initialize(routeMatch.Controller, routeMatch.Action, routeMatch.PathParameters, context);

                controller.InvokeRequestedAction();

                var view = new View(context.Request.PhysicalApplicationPath, controller.ControllerName, controller.ActionName, controller.LayoutName);

                context.Response.Write(view.Run(controller.Context));
            }
            else
            {
                throw new FileNotFoundException();
            }

            sw.Stop();

            context.Response.Write("<p>" + sw.Elapsed.TotalSeconds + "</p>");
        }

        public bool IsReusable
        {
            get { return true; }
        }

        #endregion
    }
}
