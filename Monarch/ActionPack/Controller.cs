using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using Monarch.ActiveSupport;

namespace Monarch.ActionPack
{
    public abstract class Controller
    {
        #region Fields

        private ViewDictionary data = new ViewDictionary();
        private string layoutName = "Application";

        #endregion

        #region Instance Properties

        public ViewDictionary Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        public HttpContext HttpContext
        {
            get; set;
        }

        public string LayoutName
        {
            get { return layoutName; }
            set { layoutName = value; }
        }

        public NameValueCollection Parameters
        {
            get; set;
        }

        public HttpRequest Request
        {
            get
            {
                return HttpContext.Request;
            }
        }

        public HttpResponse Response
        {
            get
            {
                return HttpContext.Response;
            }
        }

        internal string ActionName
        {
            get; set;
        }

        internal string ControllerName
        {
            get; set;
        }

        internal IDictionary<string, string> PathParameters
        {
            get; set;
        }

        internal MethodInfo RequestedAction
        {
            get; set;
        }

        #endregion

        #region Instance Methods

        public string GetParam(string key)
        {
            string toReturn;

            PathParameters.TryGetValue(key, out toReturn);

            if (null == toReturn)
                toReturn = Request.QueryString[key];

            if (null == toReturn)
                toReturn = Request.Form[key];

            return toReturn;
        }

        protected void Add(object value)
        {
            var name = value.GetType().Name;

            if (name.EndsWith("[]"))
                name = Inflector.Pluralize(name.Remove(name.Length - 2));

            Add(Inflector.Uncapitalize(name), value);
        }

        protected void Add(string key, object value)
        {
            Data.Add(key, value);
        }

        internal void InvokeRequestedAction()
        {
            RequestedAction.Invoke(this, GetMethodParameters(RequestedAction));
        }

        private object[] GetMethodParameters(MethodInfo method)
        {
            // Make up the list of parameters
            var methodParams = method.GetParameters();
            var paramList = new List<object>();

            foreach (var param in methodParams)
            {
                // Convert request param to correct type
                object convertedValue = null;

                if (param.ParameterType == typeof(HttpPostedFile))
                {
                    // Get the posted file from the files collection
                    convertedValue = Request.Files[param.Name];
                }
                else if (param.ParameterType == typeof(Guid))
                {
                    // Get the posted file from the files collection
                    var valueAsString = GetParam(param.Name);

                    convertedValue = valueAsString == null ? Guid.Empty : new Guid(valueAsString);
                }
                else
                {
                    // Convert value to correct type
                    var valueAsString = GetParam(param.Name);

                    if (param.ParameterType.IsArray)
                    {
                        // This is an array
                        if (valueAsString.Length > 0)
                        {
                            var array = valueAsString.Split(',');
                            convertedValue = ArrayConverter.ConvertStringArray(array, param.ParameterType);
                        }
                    }
                    else
                    {
                        // Normal type (not an array)
                        try
                        {
                            convertedValue = Convert.ChangeType(valueAsString, param.ParameterType);
                        }
                        catch (Exception)
                        {
                            convertedValue = null;
                        }
                    }
                }

                if (convertedValue == null)
                {
                    // The converted value is null, create a new instance of this type
                    // to ensure we are using the defaults for value types.
                    if (param.ParameterType.IsValueType)
                    {
                        convertedValue = Activator.CreateInstance(param.ParameterType);
                    }
                }

                // Add converted value to param list
                paramList.Add(convertedValue);
            }

            return paramList.ToArray();
        }

        #endregion

        #region Class Methods

        internal static Controller Initialize(string controllerName, string actionName, IDictionary<string, string> pathParameters, HttpContext httpContext)
        {
            var controllerType = BuildManager.GetType(string.Format(Configuration.ApplicationNamespace + ".Controllers.{0}Controller", controllerName), false);
            var toReturn = Activator.CreateInstance(controllerType) as Controller;

            if (null != toReturn)
            {
                toReturn.RequestedAction = controllerType.GetMethod(actionName);

                if (null == toReturn.RequestedAction)
                    throw new FileNotFoundException();

                toReturn.ControllerName = controllerName;
                toReturn.ActionName = actionName;
                toReturn.HttpContext = httpContext;
                toReturn.PathParameters = pathParameters;

                // toReturn.BuildParameters();
            }

            return toReturn;
        }

        #endregion
    }
}
