using SimAGS.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ikvm.extensions;
using java.rmi.registry;
using java.util;
using SimAGS.Handlers;

namespace SimAGS
{
    /// <summary>
    /// Adapters :)
    /// </summary>
    public static class ExtetsionHandlers
    {
        #region srting
        public static string trim(this string str)
        {
            return str.Trim();
        }

        private static string RemoveHiddenCharacter(string s)
        {
            //return s.Replace("\u200e", string.Empty).Replace("\u200f", string.Empty).Replace(" ", string.Empty);
            return Regex.Replace(s, @"[^\da-zA-Z0-1]", string.Empty).ToUpper();
        }

        public static bool _equalsIgnoreCase(this String str1, String str2)
        {
            str1 = RemoveHiddenCharacter(str1);
            str2 = RemoveHiddenCharacter(str2);
            //
            //if (str1.Length != str2.Length)
            //    return false;
            //
            //for (int i = 0; i < str1.Length; i++)
            //{
            //    if (str1[i] != str2[i])
            //    {
            //        return false;
            //    }
            //}
            //
            //return true;
            //    string.Compare(l2,l1,StringComparison.OrdinalIgnoreCase) == 0,
            //};
            var res = str1.Equals(str2, StringComparison.CurrentCultureIgnoreCase);
            return res;
        }

        public static bool startsWith(this string str, string val)
        {
            return str.StartsWith(val);
        }
        public static bool isEmpty(this string str)
        {
            return String.IsNullOrEmpty(str);
        }
        #endregion

        #region srting[]
        public static string _(this string str)
        {
            return str.Trim();
        }
        #endregion

        #region <T[,]>
        public static int rows<T>(this T[,] array)
        {
            if (array.Rank == 2)
            {
                return array.GetUpperBound(0) - array.GetLowerBound(0) + 1;
            }
            throw new Exception("Array rank must eq 2.");
        }
        public static int columns<T>(this T[,] array)
        {
            if (array.Rank == 2)
            {
                return array.GetUpperBound(1) - array.GetLowerBound(1) + 1;
            }
            throw new Exception("Array rank must eq 2.");
        }
        #endregion

        #region <T[]>

        public static int length<T>(this T[] s)
        {
            return s.Length;
        }

        #endregion

        #region IList
        public static void add<T>(this IList<T> s, T v)
        {
            s.Add(v);
        }
        public static void add<T>(this IList<T> s, int i, T v)
        {
            //TODO Use i
            s.Add(v);
        }
        public static T removeFirst<T>(this IList<T> s)
        {
            T el = s[0];
            s.RemoveAt(0);
            return el;
        }
        public static int size(this IList s)
        {
            return s.Count;
        }
        public static T get<T>(this IList<T> s, int i)
        {
            return s[i];
        }
        public static bool isEmpty<T>(this IList<T> s)
        {
            return !s.Any();
        }
        #endregion

        #region TreeNode

        public static bool _equalsIgnoreCase(this TreeNode node, string name)
        {
            return node?.Text.Equals(name, StringComparison.CurrentCultureIgnoreCase) ?? false;
        }

        #endregion

        #region DataRowCollection

        public static void AddElem(this DataRowCollection rows, AbstractTableViewing elem)
        {
            rows.Add(elem.AsArrayForRow());
        }
        #endregion
    }

    public class Integer
    {
        public static int parseInt(string s)
        {
            return int.Parse(s);
        }

        public static int valueOf(string s)
        {
            return int.Parse(s);
        }
    }

    //public class JFile
    //{
    //    public JFile(string path)
    //    {
    //        Body = File.ReadAllLines(path);
    //    }
    //    public string[] Body { get; set; }
    //    public int Position { get; set; } = 0;
    //
    //    public string readLine()
    //    {
    //        var str = Position < Body.Length ? Body[Position] : null;
    //        Position++;
    //        return str;
    //    }
    //
    //    public void close()
    //    {
    //        // =)
    //    }
    //}

    public class simException : Exception
    {
        public simException() : base()
        {

        }
        public simException(string message) : base(message)
        {

        }
        public simException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }

    public class _String
    {
        public static string valueOf(object obj)
        {
            return obj?.ToString() ?? "null";
        }
        public static String format(String format, params Object[] args)
        {
            var argsS = args.Select(e => e.ToString()).ToList();
            var templ = format.Replace("%n", "\n");
            var arr = Regex.Split(templ, @"%.?.?.?.?.?[f|s|S|b|B|c|C|d|D|e|E|g|G|t|T]").ToList();
            return string.Join("", arr.Select(a =>
            {
                var i = arr.IndexOf(a);
                return (i < argsS.Count) ? a + argsS[i] : a;
            }).ToList());
        }

        public static String format(Locale loc, String format, params Object[] args)
        {
            return java.lang.String.format(loc, format, args);
        }
    }
}
