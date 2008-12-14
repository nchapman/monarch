using System;

namespace Monarch.ActiveSupport
{
    public static class ArrayConverter
    {
        #region Class Methods

        public static Array ConvertStringArray(string[] array, Type type)
        {
            if (type == typeof(string[]))
            {
                return array;
            }
            else if (type == typeof(int[]))
            {
                return Array.ConvertAll(array, new Converter<string, int>(StringToInt));
            }
            else if (type == typeof(bool[]))
            {
                return Array.ConvertAll(array, new Converter<string, bool>(StringToBool));
            }
            else
            {
                return null;
            }
        }

        private static bool StringToBool(string s)
        {
            return bool.Parse(s);
        }

        private static int StringToInt(string s)
        {
            return int.Parse(s);
        }

        #endregion
    }
}
