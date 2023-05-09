using System;
using System.Collections.Generic;
using System.Text;

namespace EstimatR
{
    public class EstimatorInput<A, B> where A : EstimatorCollection where B : EstimatorCollection
    {
        public EstimatorCollection X;
        public EstimatorCollection Y;


        //czy dawac pusty????
        public EstimatorInput()
        {
            X = new EstimatorCollection();
            Y = new EstimatorCollection();
        }

        public EstimatorInput(A x, B y)
        {
            if (x.Count != y.Count)
            {
                throw new StatisticsExceptions(StatisticsExceptionList.DataTypeInconsistentXY);
            }

            X = x;
            Y = y;
        }
    }
}
