using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Monarch.ActiveSupport
{
    public static class Reflection
    {
        #region Class Methods

        public static T[] GetInstancesByFullName<T>(string pattern)
        {
            var toReturn = new List<T>();

            foreach (var type in GetTypesByFullName(pattern))
                if (type.BaseType == typeof(T))
                    toReturn.Add((T) Activator.CreateInstance(type));

            return toReturn.ToArray();
        }

        public static Type[] GetTypesByFullName(string pattern)
        {
            var toReturn = new List<Type>();
            var regex = new Regex(pattern);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in assembly.GetTypes())
                    if (regex.IsMatch(type.FullName))
                        toReturn.Add(type);

            return toReturn.ToArray();
        }

        #endregion
    }
}
