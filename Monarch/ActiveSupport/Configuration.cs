using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Monarch.ActiveSupport
{
    class Configuration
    {
        #region Readonly & Static Fields

        private static ICollection<string> rubySearchPath;

        #endregion

        #region Class Properties

        public static string ApplicationNamespace
        {
            get
            {
                return ConfigurationManager.AppSettings["Monarch.ApplicationNamespace"];
            }
        }

        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings["Monarch.ConnectionString"];
            }
        }

        public static ICollection<string> RubySearchPath
        {
            get
            {
                if (null == rubySearchPath)
                {
                    var rubySearchPathString = ConfigurationManager.AppSettings["Monarch.RubySearchPath"];

                    if (string.IsNullOrEmpty(rubySearchPathString))
                    {
                        var systemPaths = new[] {@"C:\ruby\lib\ruby\1.8"};

                        foreach (var path in systemPaths)
                        {
                            if (Directory.Exists(path))
                            {
                                rubySearchPathString = path;
                                break;
                            }
                        }
                    }

                    rubySearchPath = rubySearchPathString == null ? null : rubySearchPathString.Split(";".ToCharArray());
                }

                return rubySearchPath;
            }
        }

        #endregion
    }
}