using Monarch.ActiveSupport;

namespace Monarch.ActionPack.Helpers
{
    public class Helper
    {
        #region Readonly & Static Fields

        private static Helper[] userDefinedHelpers;

        #endregion

        #region Instance Properties

        internal string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        #endregion

        #region Class Methods

        public static Helper[] GetUserDefinedHelpers()
        {
            if (null == userDefinedHelpers) 
                userDefinedHelpers = Reflection.GetInstancesByFullName<Helper>(@"\.Helpers\.([^\.]+)Helper$");

            return userDefinedHelpers;
        }

        #endregion
    }
}
