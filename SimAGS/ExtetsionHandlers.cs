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
        #region double[,]
        public static void setQuick(this double[,] matr, int x, int y, double val)
        {
            matr[x, y] = val;
        }

        public static double getQuick(this double[,] matr, int x, int y)
        {
            return matr[x, y];
        }
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

        #region IList
        public static void add<T>(this IList<T> s, T v)
        {
            s.Add(v);
        }
        public static int size(this IList s)
        {
            return s.Count;
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
