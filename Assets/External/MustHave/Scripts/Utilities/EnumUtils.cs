using System;
using System.Collections.Generic;
using System.Linq;

namespace MustHave.Utilities
{
    public struct EnumUtils
    {
        public static List<string> GetNamesList<T>()
        {
            string[] names = Enum.GetNames(typeof(T));
            return new List<string>(names);
        }

        public static T[] GetValues<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static List<T> GetList<T>()
        {
            return GetValues<T>().ToList();
        }

        public static void AddEnumNamesWithPrefixToFloatDictionary<T>(Dictionary<string, float> dict, string prefix)
        {
            List<string> names = EnumUtils.GetNamesList<T>();
            foreach (var name in names)
            {
                if (name.StartsWith(prefix) && !dict.ContainsKey(name))
                {
                    dict.Add(name, 0);
                }
            }
        }

        public static void AddEnumNamesWithPrefixToList<T>(List<string> list, string prefix)
        {
            List<string> names = EnumUtils.GetNamesList<T>();
            names = names.FindAll(name => name.StartsWith(prefix) && !list.Contains(name));
            list.AddRange(names);
        }

        public static void AddEnumNamesToList<T>(List<string> list)
        {
            List<string> names = EnumUtils.GetNamesList<T>();
            names = names.FindAll(name => !list.Contains(name));
            list.AddRange(names);
        }
    }
}
