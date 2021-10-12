using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common
{
    public static class M
    {
        public static void ForEach<T>(this IEnumerable<T> a, Action<T> func)
        {
            foreach (var b in a)
                func(b);
        }

        public static IEnumerable<string> GetFilePaths(this string path)
        {
            var a = new DirectoryInfo(path);
            List<string> g = new();
            
            void Find(DirectoryInfo d)
            {
                d.GetFiles().Where(t => Regex.IsMatch(t.FullName, ".*\\.mira")).Select(t => t.FullName).ForEach(g.Add);
                d.GetDirectories().ForEach(Find);
            }

            Find(a);
            return g;
        }

        public static bool IsIdentifierLetter(this char c)
        {
            return char.IsLetterOrDigit(c) || c is '_';
        }

        public static bool Match<T1, T2>(this IEnumerable<T1> col1, IEnumerable<T2> col2, Func<T1, T2, bool> eq)
        {
            var k2 = col2.ToArray();
            foreach (var e1 in col1) {
                if (!k2.Any(t => eq(e1, t))) {
                    return false;
                }
                k2 = k2.Where(t => !eq(e1, t)).ToArray();
            }
            return k2.Length == 0;
        }
        public static bool Match<T1, T2>(this IEnumerable<T1> col1, IEnumerable<T2> col2)
        {
            return col1.Match(col2, (t1, t2) => t1 is null ? t2 is null : t1.Equals(t2));
        }

        public static bool IsPrintable(this char c)
        {
            switch (char.GetUnicodeCategory(c)) {
                case UnicodeCategory.Control:
                case UnicodeCategory.Surrogate:
                case UnicodeCategory.Format:
                case UnicodeCategory.OtherNotAssigned:
                    return true;
                default:
                    return false;
            }
        }
        public static string LuaStringify(this char a)
        {
            return a.IsPrintable() ? ((int)a).ToString() : a.ToString();
        }
        public static string LuaStringify(this string s)
        {
            return string.Concat(s.Select(t => t.LuaStringify()));
        }
        public static string StringifyLuaIndex(object a)
        {
            if (a is string a1)
                return LuaStringify(a1);
            if (a is int a2)
                return a2.ToString();
            throw new();
        }

        // public static int GetHashCode(params object[] a)
        // {
        //     
        // }
    }
}
