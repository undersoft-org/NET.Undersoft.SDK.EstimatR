using System;
using System.Collections.Generic;
using System.Text;

namespace EstimatR
{
    public class StatisticsExceptions : Exception
    {
        public StatisticsExceptions(StatisticsExceptionList exceptionList)
           : base(StatisticsExceptionRegistry.Registry[exceptionList])
        {

        }
    }

    public static class StatisticsExceptionRegistry
    {
        public static Dictionary<StatisticsExceptionList, string> Registry =
            new Dictionary<StatisticsExceptionList, string>()
        {
            {StatisticsExceptionList.DataType, "Wrong input data type" },
            {StatisticsExceptionList.DataTypeSingle, "Wrong data type: input X or Y is not a single column" },
            {StatisticsExceptionList.DataTypeConvertDouble, "Wrong data type: input cannot be converted to double" },
            {StatisticsExceptionList.DataTypeInconsistentXY, "Wrong data type: input X inconsistent with Y" },
            {StatisticsExceptionList.InputParameterInconsistent, "Input inconsistent estimator parameter size" },
            {StatisticsExceptionList.MethodCannotBeProceeded, "Method cannot be proceeded for this estimator" },
            {StatisticsExceptionList.Error, "Error - System Crash" }
        };

    }

    public enum StatisticsExceptionList
    {
        DataType,
        DataTypeSingle,
        DataTypeConvertDouble,
        DataTypeInconsistentXY,
        InputParameterInconsistent,
        MethodCannotBeProceeded,
        Error
    }
}
