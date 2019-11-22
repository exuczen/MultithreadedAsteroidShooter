using System.Collections.Generic;
using UnityEngine;

namespace MustHave.Utilities
{
    public struct ListUtils
    {
        public static string ToString<T>(List<T> list)
        {
            string s = "";
            for (int i = 0; i < list.Count; i++)
            {
                s += list[i].ToString() + ", ";
            }
            return s;
        }

        public static List<int> CreateIntList(int beg, int count)
        {
            List<int> list = new List<int>();
            int end = beg + count;
            for (int i = beg; i < end; i++)
            {
                list.Add(i);
            }
            return list;
        }

        public static void PrintList<T>(string prefix, List<T> list)
        {
            Debug.Log("ListUtils.PrintList: " + prefix + ToString(list));
        }

        public static bool CompareLists<T>(List<T> listA, List<T> listB)
        {
            if (listA.Count == listB.Count)
            {
                for (int i = 0; i < listA.Count; i++)
                {
                    if (!listA[i].Equals(listB[i]))
                        return false;
                }
                return true;
            }
            return false;
        }

        public static List<T> JoinLists<T>(List<List<T>> lists)
        {
            List<T> destList = new List<T>();
            foreach (var list in lists)
            {
                destList.AddRange(list);
            }
            return destList;
        }
    }
}
