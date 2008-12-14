using System.Text.RegularExpressions;

namespace Monarch.ActionPack
{
    class Route
    {
        #region Instance Properties

        public string Action { get; set; }
        public string Controller { get; set; }
        public Regex PatternRegex { get; set; }

        #endregion
    }
}
