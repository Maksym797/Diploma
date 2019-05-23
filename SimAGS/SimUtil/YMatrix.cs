using SimAGS.Components;
using SimAGS.PfProcessor;

namespace SimAGS.SimUtil
{
    public class yMatrix
    {
        public int nYNode = 0; // number of nodes in Y matrix x
        public double[,] yMatRe; // real part of system admittance matrix 
        public double[,] yMatIm; // imaginary part of system admittance matrix 
        public PFCase pfCaseMaseter = new PFCase();

        // busList --> array list storing sorted buses 
        public yMatrix(PFCase pfCaseMain, bool bIncludeLoadY)
        {
            pfCaseMaseter = pfCaseMain;

            if ((nYNode = pfCaseMaseter.sortBusArrayList.Count) != 0)
            {
                //  yMatRe = new Sparsedouble[,](nYNode, nYNode);
                //  yMatIm = new Sparsedouble[,](nYNode, nYNode);

                //  //--------------------- add bus admittance -----------------------//
                //  for (bus busTemp: pfCaseMaseter.sortBusArrayList)
                //  {
                //      busTemp.updateYMat(yMatRe, yMatIm);
                //  }

                //  //--------------------- add branch admittance -------------------//
                //  for (branch branchTemp: pfCaseMaseter.branchArrayList)
                //  {
                //      branchTemp.updateYMat(yMatRe, yMatIm);
                //  }

                //  //------------------- add transformer admittance -----------------//
                //  for (twoWindTrans transTemp : pfCaseMaseter.twoWindTransArrayList)
                //  {
                //      transTemp.updateYMat(yMatRe, yMatIm);
                //  }

                //  //------------------- display Y matrix element ------------------//
                //  /*
                //  String strTemp = ""; 
                //  for (int i=0;i<nYNode;i++){
                //      for (int j=0;j<nYNode;j++){
                //          if (yMatRe.getQuick(i,j)!=0 || yMatIm.getQuick(i,j)!=0){
                //              strTemp = "("+ nodeList.get(i) + "," + nodeList.get(j)+")\t = " + String.format("%1$.4f", yMatRe.getQuick(i,j)) + "+j" + String.format("%1$.4f", yMatIm.getQuick(i,j));
                //              System.out.println(strTemp);
                //          }
                //      }
                //  }
                //  */
            }
        }


        // modify yMat element 
        public void addBusImpd(int busNum, double addR, double addX)
        {
              bus busTemp = dataProcess.getBusAt(busNum, pfCaseMaseter.sortBusArrayList);
              if (busTemp != null)
              {
                  double addG = addR / (addR * addR + addX * addX);
                  double addB = -addX / (addR * addR + addX * addX);
                  yMatRe.setQuick(busTemp.yMatIndx, busTemp.yMatIndx, yMatRe.getQuick(busTemp.yMatIndx, busTemp.yMatIndx) + addG);
                  yMatIm.setQuick(busTemp.yMatIndx, busTemp.yMatIndx, yMatIm.getQuick(busTemp.yMatIndx, busTemp.yMatIndx) + addB);
              }
        }

        // convert load to constant-impedance load 
        //public void removeCYLoad(ArrayList<bus> busList){
        //	for (bus busTemp: busList) {
        //		if (busTemp.bHasLoad == true) {
        //			yMatRe.setQuick(busTemp.yMatIndx,busTemp.yMatIndx,yMatRe.getQuick(busTemp.yMatIndx,busTemp.yMatIndx) - busTemp.aggCYLoadP);		
        //			yMatIm.setQuick(busTemp.yMatIndx,busTemp.yMatIndx,yMatIm.getQuick(busTemp.yMatIndx,busTemp.yMatIndx) - busTemp.aggCYLoadQ);
        //			// all adjustments to yMat and network jacobian matrix will be done through DYNLOAD 
        //			System.out.println("ConstY load at bus " + busTemp.I + " is removed from Ymat"); 
        //		}
        //	}
        //}


        //remove the branch from YMat
        public void removeBranch(branch branTemp)
        {
            int frBusYIndx = branTemp.frBus.yMatIndx;
            int toBusYIndx = branTemp.toBus.yMatIndx;

            if (branTemp.ST == 1)
            {
                // branch is closed at power flow case; otherwise skip
                yMatRe.setQuick(frBusYIndx, frBusYIndx, yMatRe.getQuick(frBusYIndx, frBusYIndx) - branTemp.calcGII);
                yMatRe.setQuick(frBusYIndx, toBusYIndx, yMatRe.getQuick(frBusYIndx, toBusYIndx) - branTemp.calcGIJ);
                yMatRe.setQuick(toBusYIndx, frBusYIndx, yMatRe.getQuick(toBusYIndx, frBusYIndx) - branTemp.calcGJI);
                yMatRe.setQuick(toBusYIndx, toBusYIndx, yMatRe.getQuick(toBusYIndx, toBusYIndx) - branTemp.calcGJJ);

                yMatIm.setQuick(frBusYIndx, frBusYIndx, yMatIm.getQuick(frBusYIndx, frBusYIndx) - branTemp.calcBII);
                yMatIm.setQuick(frBusYIndx, toBusYIndx, yMatIm.getQuick(frBusYIndx, toBusYIndx) - branTemp.calcBIJ);
                yMatIm.setQuick(toBusYIndx, frBusYIndx, yMatIm.getQuick(toBusYIndx, frBusYIndx) - branTemp.calcBJI);
                yMatIm.setQuick(toBusYIndx, toBusYIndx, yMatIm.getQuick(toBusYIndx, toBusYIndx) - branTemp.calcBJJ);
            }
        }
    }
}
