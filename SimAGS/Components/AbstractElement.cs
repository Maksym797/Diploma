using SimAGS.Components.ExtendOption;
using System;
using System.Collections.Generic;
using SimAGS.Handlers;

namespace SimAGS.Components
{
    public abstract class abstractPfElement : AbstractTableViewing
    {
        public const int SBASE = 100;
        public const double Deg2Rad = Math.PI / 180;

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