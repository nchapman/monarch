using System.Collections.Generic;

namespace Monarch.ActionPack
{
    class RouteMatch
    {
        #region Instance Properties

        public string Action { get; set; }
        public string Controller { get; set; }
        public Dictionary<string, string> PathParameters { get; set; }
        public bool Success { get; set; }

        #endregion
    }
}
