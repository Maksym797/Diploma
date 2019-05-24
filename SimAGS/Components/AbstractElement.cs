using SimAGS.Components.ExtendOption;
using System;
using System.Collections.Generic;

namespace SimAGS.Components
{
    public abstract class abstractPfElement
    {
        public const int SBASE = 100;
        public const double Deg2Rad = Math.PI / 180;

        public abstract string[] AsArrayForRow();

        public BaseExtOption getVoltExtOption()
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