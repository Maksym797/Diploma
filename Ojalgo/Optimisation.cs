using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ojalgo
{
    public class Optimisation
    {
        public class Result
        {
        //    //private const Access1D<?> mySolution;
        //    //private final Optimisation.State myState;
        //    //private final double myValue; // Objective Function Value

        //    public Result(Optimisation.State state, double value, Access1D<?> solution)
        //    {

        //        super();

        //        ProgrammingError.throwIfNull(state);
        //        ProgrammingError.throwIfNull(solution);

        //        myState = state;
        //        myValue = value;
        //        mySolution = solution;
        //    }

        //    public int compareTo(final Result reference)
        //    {

        //        final double tmpReference = reference.getValue();

        //        if (myValue > tmpReference)
        //        {
        //            return 1;
        //        }
        //        else if (myValue < tmpReference)
        //        {
        //            return -1;
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }

        //    public double doubleValue(final int anInd)
        //    {
        //        return mySolution.doubleValue(anInd);
        //    }

        //    @Override
        //public boolean equals(final Object obj)
        //    {
        //        if (this == obj)
        //        {
        //            return true;
        //        }
        //        if (obj == null)
        //        {
        //            return false;
        //        }
        //        if (this.getClass() != obj.getClass())
        //        {
        //            return false;
        //        }
        //        final Result other = (Result)obj;
        //        if (myState != other.myState)
        //        {
        //            return false;
        //        }
        //        if (Double.doubleToLongBits(myValue) != Double.doubleToLongBits(other.myValue))
        //        {
        //            return false;
        //        }
        //        return true;
        //    }

            public double Get(int anInd)
            {
                throw new NotImplementedException();
                //return TypeUtils.toBigDecimal(mySolution.get(anInd));
            }

        //    /**
        //     * @deprecated v33 Use {@linkplain #get(int)} or {@linkplain #doubleValue(int)} instead.
        //     */
        //    @Deprecated
        //public BasicMatrix getSolution()
        //    {
        //        return BigMatrix.FACTORY.columns(this);
        //    }

        //    public Optimisation.State getState()
        //    {
        //        return myState;
        //    }

        //    public double getValue()
        //    {
        //        return myValue;
        //    }

        //    @Override
        //    public int hashCode()
        //    {
        //        final int prime = 31;
        //        int result = 1;
        //        result = (prime * result) + ((myState == null) ? 0 : myState.hashCode());
        //        long temp;
        //        temp = Double.doubleToLongBits(myValue);
        //        result = (prime * result) + (int)(temp ^ (temp >>> 32));
        //        return result;
        //    }

        //    public Iterator<BigDecimal> iterator()
        //    {
        //        return new Iterator1D<BigDecimal>(this);
        //    }

        //    public int size()
        //    {
        //        return mySolution.size();
        //    }

        //    @Override
        //public String toString()
        //    {
        //        return myState + " " + myValue + " @ " + PrimitiveDenseStore.FACTORY.columns(mySolution);
        //    }
        }
        public class State
        {

            /**
             * Approximate and/or Intermediate solution - Iteration point
             * Probably infeasible, but still "good"
             */
            public State APPROXIMATE = new State(8, 32);

            /**
             * Unique (and optimal) solution - there is no other solution that
             * is equal or better
             */
            public State DISTINCT = new State(256, 112);

            /**
             * Failed
             */
            public State FAILED = new State(-1, -128);

            /**
             * Solved - a solution that complies with all constraints
             */
            public State FEASIBLE = new State(16, 64);

            /**
             * Optimal, but not distinct solution - there are other solutions that
             * are equal, but not better.
             */
            public State INDISTINCT = new State(-128, 96);

            /**
             * No solution that complies with all constraints exists
             */
            public State INFEASIBLE = new State(-8, -64);

            /**
             * Model/problem components/entities are invalid
             */
            public State INVALID = new State(-2, -128);

            /**
             * Approximate and/or Intermediate solution - Iteration point
             * Probably infeasible, but still "good"
             * @deprecated v33 Use APPROXIMATE instead
             */
            public State ITERATION = new State(8, 32);

            /**
             * New/changed problem
             * @deprecated v33 Use UNEXPLORED instead
             */
            public State NEW = new State(0, 0);

            /**
             * Optimal solution - there is no better
             */
            public State OPTIMAL = new State(64, 96);

            /**
             * There's an infinite number of feasible solutions and no bound on the objective function value
             */
            public State UNBOUNDED = new State(-32, -32);

            /**
             * New/changed problem
             */
            public State UNEXPLORED = new State(0, 0);

            /**
             * Unique (and optimal) solution - there is no other solution that
             * is equal or better
             * @deprecated v33 Use DISTINCT instead
             */
            public State UNIQUE = new State(256, 112);

            /**
             * Model/problem components/entities are valid
             */
            public State VALID = new State(4, 0);

            private readonly int myByte;
            private readonly int myValue;
            //
            State(int aValue, int aByte)
            {
                myValue = aValue;
                myByte = aByte;
            }

            public bool isDistinct()
            {
                return this.absValue() >= DISTINCT.absValue();
            }

            public bool isExactly(State aState)
            {
                return myByte == aState.byteValue();
            }

            public bool isFailure()
            {
                return myValue < 0;
            }

            public bool isFeasible()
            {
                return this.absValue() >= FEASIBLE.absValue();
            }

            public bool isLessThan(State aState)
            {
                return myByte < aState.byteValue();
            }

            public bool isMoreThan(State aState)
            {
                return myByte > aState.byteValue();
            }

            public bool isNotLessThan(State aState)
            {
                return myByte >= aState.byteValue();
            }

            public bool isNotMoreThan(State aState)
            {
                return myByte <= aState.byteValue();
            }

            public bool isOptimal()
            {
                return this.absValue() >= OPTIMAL.absValue();
            }

            /**
             * VALID, APPROXIMATE, FEASIBLE, OPTIMAL or DISTINCT
             */
            public bool isSuccess()
            {
                return myValue > 0;
            }

            /**
             * UNEXPLORED
             */
            public bool isUnexplored()
            {
                return myValue == 0;
            }

            public bool isValid()
            {
                return this.absValue() >= VALID.absValue();
            }

            private int absValue()
            {
                return Math.Abs(myValue);
            }

            private int byteValue()
            {
                return myByte;
            }

        }
    }
}
