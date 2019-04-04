using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using SimAGS.Components.ExtendOption;

namespace SimAGS.Components
{
    internal abstract class AbstractElement
    {
        public const int SBase = 100;
        public const double Deg2Ras = Math.PI / 180;

        public BaseExtOption GetVoltExtOption()
        {
            return new BaseExtOption();
        }

        public BaseExtOption getMWExtOption()
        {
            return new BaseExtOption();
        }

        //public abstract void UpdateYMAt(Matrix<double> YMatRe, Matrix<double> YMatIm);
    }
}