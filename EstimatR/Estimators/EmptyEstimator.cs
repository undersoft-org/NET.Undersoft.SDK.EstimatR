using System;
using System.Collections.Generic;
using System.Text;

namespace EstimatR
{
    public class EmptyEstimator : Estimator
    {
        public override void Prepare(EstimatorInput<EstimatorCollection, EstimatorCollection> input)
        {
            Input = input;
        }

        public override void Prepare(EstimatorCollection x, EstimatorCollection y)
        {
            Input = new EstimatorInput<EstimatorCollection, EstimatorCollection>(x, y);
        }

        public override void Create()
        {

        }

        public override EstimatorItem Evaluate(object x)
        {
            return Evaluate(new EstimatorItem(x));
        }

        public override EstimatorItem Evaluate(EstimatorItem x)
        {
            return new EstimatorItem(x);
        }

        public override void Update(EstimatorInput<EstimatorCollection, EstimatorCollection> input)
        {
            return;
        }

        public override void Update(EstimatorCollection x, EstimatorCollection y)
        {
            return;
        }

        public override double[][] GetParameters()
        {
            return null;
        }
    }

}
