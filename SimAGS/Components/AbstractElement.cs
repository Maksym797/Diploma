using SimAGS.Components.ExtendOption;
using System;
using System.Collections.Generic;
using SimAGS.Handlers;

namespace SimAGS.Components
{
    public abstract class abstractPfElement : IHasTableView
    {
        public const int SBASE = 100;
        public const double Deg2Rad = Math.PI / 180;

        public abstract static string[] he { get; set; }

        public abstract string[] AsArrayForRow();
        public abstract string[] GetHeaders();

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