using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimAGS
{
    /// <summary>
    /// Adapters :)
    /// </summary>
    public static class ExtetsionHandlers
    {
        #region DoubleMatrix2D
        //public static void setQuick(this DoubleMatrix2D matr, int x, int y, double val)
        //{
        //    matr[x, y] = val;
        //}
        //
        //public static double getQuick(this DoubleMatrix2D matr, int x, int y)
        //{
        //    return matr[x, y];
        //}
        #endregion

        #region srting
        public static string trim(this string str)
        {
            return str.Trim();
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
        public static int size(this IList s)
        {
            return s.Count;
        }
        public static T get<T>(this IList<T> s, int i)
        {
            return s[i];
        }
        #endregion

    }

    public class Integer
    {
        public static int parseInt(string s)
        {
            return int.Parse(s);
        }
    }

    public class JFile
    {
        public JFile(string path)
        {
            Body = File.ReadAllLines(path);
        }
        public string[] Body { get; set; }
        public int Position { get; set; } = 0;

        public string readLine()
        {
            var str = Position < Body.Length ? Body[Position] : null ;
            Position++;
            return str;
        }

        public void close()
        {
            // =)
        }
    }

    public class simException: Exception
    {
        public simException() : base()
        {
            
        }
        public simException(string message) : base(message)
        {

        }
    }
}
