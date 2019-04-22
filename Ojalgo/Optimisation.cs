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
            private const Access1D<?> mySolution;
            private final Optimisation.State myState;
            private final double myValue; // Objective Function Value

            public Result(final Optimisation.State state, final double value, final Access1D<?> solution)
            {

                super();

                ProgrammingError.throwIfNull(state);
                ProgrammingError.throwIfNull(solution);

                myState = state;
                myValue = value;
                mySolution = solution;
            }

            public int compareTo(final Result reference)
            {

                final double tmpReference = reference.getValue();

                if (myValue > tmpReference)
                {
                    return 1;
                }
                else if (myValue < tmpReference)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }

            public double doubleValue(final int anInd)
            {
                return mySolution.doubleValue(anInd);
            }

            @Override
        public boolean equals(final Object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (obj == null)
                {
                    return false;
                }
                if (this.getClass() != obj.getClass())
                {
                    return false;
                }
                final Result other = (Result)obj;
                if (myState != other.myState)
                {
                    return false;
                }
                if (Double.doubleToLongBits(myValue) != Double.doubleToLongBits(other.myValue))
                {
                    return false;
                }
                return true;
            }

            public BigDecimal get(final int anInd)
            {
                return TypeUtils.toBigDecimal(mySolution.get(anInd));
            }

            /**
             * @deprecated v33 Use {@linkplain #get(int)} or {@linkplain #doubleValue(int)} instead.
             */
            @Deprecated
        public BasicMatrix getSolution()
            {
                return BigMatrix.FACTORY.columns(this);
            }

            public Optimisation.State getState()
            {
                return myState;
            }

            public double getValue()
            {
                return myValue;
            }

            @Override
        public int hashCode()
            {
                final int prime = 31;
                int result = 1;
                result = (prime * result) + ((myState == null) ? 0 : myState.hashCode());
                long temp;
                temp = Double.doubleToLongBits(myValue);
                result = (prime * result) + (int)(temp ^ (temp >>> 32));
                return result;
            }

            public Iterator<BigDecimal> iterator()
            {
                return new Iterator1D<BigDecimal>(this);
            }

            public int size()
            {
                return mySolution.size();
            }

            @Override
        public String toString()
            {
                return myState + " " + myValue + " @ " + PrimitiveDenseStore.FACTORY.columns(mySolution);
            }
        }
        public class State
        {

        /**
         * Approximate and/or Intermediate solution - Iteration point
         * Probably infeasible, but still "good"
         */
            public Tuple<int, int> APPROXIMATE = new Tuple<int, int>(8, 32);

        /**
         * Unique (and optimal) solution - there is no other solution that
         * is equal or better
         */
            public Tuple<int, int> DISTINCT = new Tuple<int, int>(256, 112);

            /**
             * Failed
             */
            public Tuple<int, int> FAILED = new Tuple<int, int>(-1, -128);

            /**
             * Solved - a solution that complies with all constraints
             */
            public Tuple<int, int> FEASIBLE = new Tuple<int, int>(16, 64);

            /**
             * Optimal, but not distinct solution - there are other solutions that
             * are equal, but not better.
             */
            public Tuple<int, int> INDISTINCT = new Tuple<int, int>(-128, 96);

            /**
             * No solution that complies with all constraints exists
             */
            public Tuple<int, int> INFEASIBLE = new Tuple<int, int>(-8, -64);

            /**
             * Model/problem components/entities are invalid
             */
            public Tuple<int, int> INVALID = new Tuple<int, int>(-2, -128);

            /**
             * Approximate and/or Intermediate solution - Iteration point
             * Probably infeasible, but still "good"
             * @deprecated v33 Use APPROXIMATE instead
             */
            public Tuple<int, int> ITERATION = new Tuple<int, int>(8, 32);

            /**
             * New/changed problem
             * @deprecated v33 Use UNEXPLORED instead
             */
            public Tuple<int, int> NEW = new Tuple<int, int>(0, 0);

            /**
             * Optimal solution - there is no better
             */
            public Tuple<int, int> OPTIMAL = new Tuple<int, int>(64, 96);

            /**
             * There's an infinite number of feasible solutions and no bound on the objective function value
             */
            public Tuple<int, int> UNBOUNDED = new Tuple<int, int>(-32, -32);

            /**
             * New/changed problem
             */
            public Tuple<int, int> UNEXPLORED = new Tuple<int, int>(0, 0);

            /**
             * Unique (and optimal) solution - there is no other solution that
             * is equal or better
             * @deprecated v33 Use DISTINCT instead
             */
            public Tuple<int, int> UNIQUE = new Tuple<int, int>(256, 112);

            /**
             * Model/problem components/entities are valid
             */
            public Tuple<int, int> VALID = new Tuple<int, int>(4, 0);

        //private final byte myByte;
        //private final int myValue;
        //
        //puState(final int aValue, final int aByte)
        //{
        //    myValue = aValue;
        //    myByte = (byte)aByte;
        //}

        public bool isDistinct()
        {
            return this.absValue() >= DISTINCT.absValue();
        }

        /**
         * 0
         * 
         * @deprecated v33
         */
        @Deprecated
        public boolean isExactly(final State aState)
        {
            return myByte == aState.byteValue();
        }

        /**
         * FAILED, INVALID, INFEASIBLE, UNBOUNDED or INDISTINCT
         */
        public boolean isFailure()
        {
            return myValue < 0;
        }

        public boolean isFeasible()
        {
            return this.absValue() >= FEASIBLE.absValue();
        }

        /**
         * 0
         * 
         * @deprecated v33
         */
        @Deprecated
        public boolean isLessThan(final State aState)
        {
            return myByte < aState.byteValue();
        }

        /**
         * 0
         * 
         * @deprecated v33
         */
        @Deprecated
        public boolean isMoreThan(final State aState)
        {
            return myByte > aState.byteValue();
        }

        /**
         * 17
         * 
         * @deprecated v33
         */
        @Deprecated
        public boolean isNotLessThan(final State aState)
        {
            return myByte >= aState.byteValue();
        }

        /**
         * 0
         * 
         * @deprecated v33
         */
        @Deprecated
        public boolean isNotMoreThan(final State aState)
        {
            return myByte <= aState.byteValue();
        }

        public boolean isOptimal()
        {
            return this.absValue() >= OPTIMAL.absValue();
        }

        /**
         * VALID, APPROXIMATE, FEASIBLE, OPTIMAL or DISTINCT
         */
        public boolean isSuccess()
        {
            return myValue > 0;
        }

        /**
         * UNEXPLORED
         */
        public boolean isUnexplored()
        {
            return myValue == 0;
        }

        public boolean isValid()
        {
            return this.absValue() >= VALID.absValue();
        }

        private int absValue()
        {
            return Math.abs(myValue);
        }

        private byte byteValue()
        {
            return myByte;
        }

    }
}
}
