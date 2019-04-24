using SimAGS.Components.ExtendOption;
using System;

namespace SimAGS.Components
{
    public abstract class AbstractElement
    {
        public const int SBASE = 100;
        public const double Deg2Rad = Math.PI / 180;

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