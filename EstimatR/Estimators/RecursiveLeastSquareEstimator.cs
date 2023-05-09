using System;
using System.Collections.Generic;
using System.Text;

namespace EstimatR
{
    public class RecursiveLeastSquareEstimator : Estimator
    {

        private bool validParameters;
        private double[][] parameterK;
        private double[][] parameterP;
        private double[][] parameterTheta;

        private List<double> advancedParameters;


        public override void Prepare(EstimatorInput<EstimatorCollection, EstimatorCollection> input)
        {
            //verification etc....
            Input = input;
            validInput = true;
            validParameters = false;
        }

        public override void Prepare(EstimatorCollection x, EstimatorCollection y)
        {
            Prepare(new EstimatorInput<EstimatorCollection, EstimatorCollection>(x, y));
        }

        public override EstimatorItem Evaluate(object x)
        {
            return Evaluate(new EstimatorItem(x));
        }

        public override EstimatorItem Evaluate(EstimatorItem x)
        {
            if (validParameters == false) //to aviod recalculations of systemParameters
            {
                Create();
            }

            //eval create, gdyz moga miec rozne rozmiary danych wejsciowych

            return new EstimatorItem(MatrixOperations.MatrixVectorProduct(MatrixOperations.MatrixTranpose(parameterTheta), x.Vector));
        }

        public override void Create()
        {
            // RLS Canonical form:
            // 
            // P initial values:
            //    a) P>>0 (10^3) small confidence in initial theta
            //    b) P~10 confidence in initial theta
            // theta = column vector
            // P = eye(nx,nx)*value
            // X = [[x1..xn];[x1,...,xn]; [x1,..., xn]]
            // Y =[
            // XX = [x1...xn]' column vector
            // K = P*XX*inv(1+XX'*P*XX) //inv == ^(-1)
            // theta = theta + K * (YY - XX'*theta)
            // P = P - K*XX'*P
            // with forgetting factor FF (usually 0.95, 0.995)
            // K = P*XX*inv(FF+XX'*P*XX) //inv == ^(-1)
            // theta = theta + K * (YY - XX'*theta)
            // P = (P - K*XX'*P)/FF


            //throw exception if covariance matrix is invertible

            if (validInput == false)
            {
                throw new StatisticsExceptions(StatisticsExceptionList.DataType);
            }

            int m = Input.X.Count;
            int nx = Input.X[0].Vector.Length;
            int ny = Input.Y[0].Vector.Length;

            double[][] xx = MatrixOperations.MatrixCreate(nx, 1);
            double[][] yy = MatrixOperations.MatrixCreate(1, ny);

            double[][] K = MatrixOperations.MatrixCreate(nx, 1);
            double[][] P = MatrixOperations.MatrixDiagonal(nx, 10000); //nx x nx small confidence in initial theta (which is 0 0 0 0)
            double[][] theta = MatrixOperations.MatrixCreate(nx, ny);
            double[][] ff = MatrixOperations.MatrixDiagonal(1, 0.95);

            //auxuliary calculations
            double[][] xxT = MatrixOperations.MatrixCreate(1, nx);    //xx'
            double[][] P_XX = MatrixOperations.MatrixCreate(nx, 1);   //P*xx
            double[][] XXT_P = MatrixOperations.MatrixCreate(1, nx);
            double[][] XXT_P_XX = MatrixOperations.MatrixCreate(1, 1); //XX'*P*XX -> scalar, later + ff
            double[][] inv_XXT_P_XX = MatrixOperations.MatrixCreate(1, 1);
            double[][] XXT_theta = MatrixOperations.MatrixCreate(1, ny);
            double[][] YY_XXT_theta = MatrixOperations.MatrixCreate(1, ny);
            double[][] K_YY_XXT_theta = MatrixOperations.MatrixCreate(nx, ny);
            double[][] K_XXT_P = MatrixOperations.MatrixCreate(nx, nx);

            xxT = MatrixOperations.MatrixTest(xxT);

            if (advancedParameters != null)
            {
                ff[0][0] = advancedParameters[0];
            }

            for (int i = 0; i < m; i++)
            {
                xx = MatrixOperations.MatrixCreateColumn(Input.X[i].Vector, xx);
                xxT = MatrixOperations.MatrixTranpose(xx, xxT);
                yy = MatrixOperations.MatrixCreateRow(Input.Y[i].Vector, yy);
                P_XX = MatrixOperations.MatrixProduct(P, xx, P_XX);
                XXT_P = MatrixOperations.MatrixProduct(xxT, P, XXT_P);
                XXT_P_XX = MatrixOperations.MatrixProduct(XXT_P, xx, XXT_P_XX);
                XXT_P_XX = MatrixOperations.MatrixSum(XXT_P_XX, ff, XXT_P_XX);
                inv_XXT_P_XX = MatrixOperations.MatrixInverse(XXT_P_XX, inv_XXT_P_XX);
                K = MatrixOperations.MatrixProduct(P_XX, inv_XXT_P_XX, K);
                XXT_theta = MatrixOperations.MatrixProduct(xxT, theta, XXT_theta);
                YY_XXT_theta = MatrixOperations.MatrixSub(yy, XXT_theta, YY_XXT_theta);
                K_YY_XXT_theta = MatrixOperations.MatrixProduct(K, YY_XXT_theta, K_YY_XXT_theta);
                theta = MatrixOperations.MatrixSum(theta, K_YY_XXT_theta, theta);
                K_XXT_P = MatrixOperations.MatrixProduct(K, XXT_P, K_XXT_P);
                P = MatrixOperations.MatrixSub(P, K_XXT_P, P);
                P = MatrixOperations.MatrixProduct(P, 1 / ff[0][0], P);
            }
            parameterK = K;
            parameterP = P;
            parameterTheta = theta;

            validParameters = true;
        }

        public override void Update(EstimatorInput<EstimatorCollection, EstimatorCollection> input)
        {
            if ((input == null || input.X.Count == 0 || input.X.Count == 0)
                || (validParameters == true)
                    && (input.X[0].Vector.Length != parameterTheta.Length || input.Y[0].Vector.Length != parameterTheta[0].Length))
            {
                throw new StatisticsExceptions(StatisticsExceptionList.InputParameterInconsistent);
            }

            int m = Input.X.Count;
            int nx = Input.X[0].Vector.Length;
            int ny = Input.Y[0].Vector.Length;

            double[][] xx = MatrixOperations.MatrixCreate(nx, 1);
            double[][] yy = MatrixOperations.MatrixCreate(1, ny);
            double[][] K = MatrixOperations.MatrixCreate(nx, 1);
            double[][] P = MatrixOperations.MatrixDiagonal(nx, 10000); //nx x nx small confidence in initial theta (which is 0 0 0 0)
            double[][] theta = MatrixOperations.MatrixCreate(nx, ny);
            double[][] ff = MatrixOperations.MatrixDiagonal(1, 0.95);

            //auxuliary calculations
            double[][] xxT = MatrixOperations.MatrixCreate(1, nx);    //xx'
            double[][] P_XX = MatrixOperations.MatrixCreate(nx, 1);   //P*xx
            double[][] XXT_P = MatrixOperations.MatrixCreate(1, nx);
            double[][] XXT_P_XX = MatrixOperations.MatrixCreate(1, 1); //XX'*P*XX -> scalar, later + ff
            double[][] inv_XXT_P_XX = MatrixOperations.MatrixCreate(1, 1);
            double[][] XXT_theta = MatrixOperations.MatrixCreate(1, ny);
            double[][] YY_XXT_theta = MatrixOperations.MatrixCreate(1, ny);
            double[][] K_YY_XXT_theta = MatrixOperations.MatrixCreate(nx, ny);
            double[][] K_XXT_P = MatrixOperations.MatrixCreate(nx, nx);

            if (validParameters != false) //update run
            {
                K = parameterK;
                P = parameterP;
                theta = parameterTheta;
            }

            if (advancedParameters != null)
            {
                ff[0][0] = advancedParameters[0];
            }

            for (int i = 0; i < m; i++)
            {
                xx = MatrixOperations.MatrixCreateColumn(Input.X[i].Vector, xx);
                xxT = MatrixOperations.MatrixTranpose(xx, xxT);
                yy = MatrixOperations.MatrixCreateRow(Input.Y[i].Vector, yy);
                P_XX = MatrixOperations.MatrixProduct(P, xx, P_XX);
                XXT_P = MatrixOperations.MatrixProduct(xxT, P, XXT_P);
                XXT_P_XX = MatrixOperations.MatrixProduct(XXT_P, xx, XXT_P_XX);
                XXT_P_XX = MatrixOperations.MatrixSum(XXT_P_XX, ff, XXT_P_XX);
                inv_XXT_P_XX = MatrixOperations.MatrixInverse(XXT_P_XX, inv_XXT_P_XX);
                K = MatrixOperations.MatrixProduct(P_XX, inv_XXT_P_XX, K);
                XXT_theta = MatrixOperations.MatrixProduct(xxT, theta, XXT_theta);
                YY_XXT_theta = MatrixOperations.MatrixSub(yy, XXT_theta, YY_XXT_theta);
                K_YY_XXT_theta = MatrixOperations.MatrixProduct(K, YY_XXT_theta, K_YY_XXT_theta);
                theta = MatrixOperations.MatrixSum(theta, K_YY_XXT_theta, theta);
                K_XXT_P = MatrixOperations.MatrixProduct(K, XXT_P, K_XXT_P);
                P = MatrixOperations.MatrixSub(P, K_XXT_P, P);
                P = MatrixOperations.MatrixProduct(P, 1 / ff[0][0], P);
            }

            parameterK = K;
            parameterP = P;
            parameterTheta = theta;

            validParameters = true;
        }

        public override void Update(EstimatorCollection x, EstimatorCollection y)
        {
            Update(new EstimatorInput<EstimatorCollection, EstimatorCollection>(x, y));
        }

        public override void SetAdvancedParameters(IList<object> advParameters = null)
        {
            //exception if zero or not double
            if (advParameters == null)
            {
                advancedParameters = null;
                return;
            }
            advancedParameters = new List<double>();
            advancedParameters.Add(Convert.ToDouble(advParameters[0]));
        }

        public override double[][] GetParameters()
        {
            return MatrixOperations.MatrixDuplicate(parameterTheta);
        }


    }

}
