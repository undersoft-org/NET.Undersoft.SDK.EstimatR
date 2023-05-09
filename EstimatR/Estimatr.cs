using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EstimatR
{
    //kazdy estymator ma swoje input - wtedy zmiana estymatora wymaga tez zmiany input - komplikacje to powoduje

    public class Estimatr : Estimator
    {
        private Estimator estimator;    //run Estimator Methods
        private EstimatorMethod defaultMethod;

        //konstruktor Statistics uzyty w IStatiscics -> implikuje zainicjowanie Input i (dodatkowo) określenie metody domyslnej estymowania
        //public Statistics(EstimatorInput<EstimatorObjectCollection, EstimatorObjectCollection> input, EstimatorMethod method = EstimatorMethod.Empty)
        public Estimatr(EstimatorInput<EstimatorCollection, EstimatorCollection> input, EstimatorMethod method)
        {
            //try catch, czy input moze byc dla danej metody !!! - zapytac Darka
            estimator = resolveMethod(method);
            defaultMethod = method;
            Prepare(input);

            //defaultowo ustawic Empty jak catch exception
        }
        //public Statistics(EstimatorObjectCollection x, EstimatorObjectCollection y, EstimatorMethod method = EstimatorMethod.Empty)
        public Estimatr(EstimatorCollection x, EstimatorCollection y, EstimatorMethod method)
        {
            estimator = resolveMethod(method);
            defaultMethod = method;
            Prepare(x, y);
        }

        //wywolywane przez konstruktory
        public override void Prepare(EstimatorInput<EstimatorCollection, EstimatorCollection> input)
        {
            try
            {
                Input = input;
                estimator.Prepare(input);       //can thow exception if input is not, e.g., 1D for LinearRegression, but input can be still valid for other estimators
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public override void Prepare(EstimatorCollection x, EstimatorCollection y)
        {
            EstimatorInput<EstimatorCollection, EstimatorCollection> _input =
                new EstimatorInput<EstimatorCollection, EstimatorCollection>(x, y);

            Prepare(_input);
        }

        public override void Create()
        {
            try
            {
                estimator.Create();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

            }
        }

        public override void Update(EstimatorInput<EstimatorCollection, EstimatorCollection> input)
        {
            try
            {
                //Input = input;  //???? czy rozszerzyć input???? czy nie???
                estimator.Update(input);       //can thow exception if input is not, e.g., 1D for LinearRegression, but input can be still valid for other estimators
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

            }

        }

        public override void Update(EstimatorCollection x, EstimatorCollection y)
        {
            EstimatorInput<EstimatorCollection, EstimatorCollection> _input =
                new EstimatorInput<EstimatorCollection, EstimatorCollection>(x, y);

            Update(_input);
        }


        public override EstimatorItem Evaluate(EstimatorItem x)
        {
            return estimator.Evaluate(x);
        }

        public override EstimatorItem Evaluate(object x)
        {
            return estimator.Evaluate(new EstimatorItem(x));
        }

        private Estimator resolveMethod(EstimatorMethod method)
        {
            return (Estimator)CallEstimatorInstance.ActivateMethod("EstimatR." + method.ToString());
        }

        public Estimator SetDefaultMethod(EstimatorMethod method)
        {
            if (estimator != null && defaultMethod == method) return estimator;

            try
            {
                Estimator newEstimator = resolveMethod(method);
                newEstimator.Prepare(Input);        //can change to this estimator for given input data
                estimator = newEstimator;
                defaultMethod = method;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return estimator;
        }

        public new void SetAdvancedParameters(IList<object> advParameters = null)
        {
            estimator.SetAdvancedParameters(advParameters);
        }

        public override double[][] GetParameters()
        {
            return estimator.GetParameters();
        }

        public EstimatorMethod GetDefaultMethod()
        {
            return defaultMethod;
        }
    }

    public enum EstimatorMethod
    {
        EmptyEstimator,
        LinearRegressionEstimator,
        LagrangeEstimator,
        LinearLastSquareEstimator,
        RecursiveLeastSquareEstimator,
        KalmanEstimator
    }
}
