namespace EstimatR
{
    public class Cluster
    {
        public double[] ClusterVector { get; set; }


        public EstimatorCollection ClusterItemList { get; set; }


        public double[] ClusterVectorSummary { get; set; }

        public Cluster(EstimatorItem item)
        {
            ClusterVector = new double[item.Vector.Length];
            Array.Copy(item.Vector, ClusterVector, item.Vector.Length);
            ClusterVectorSummary = new double[item.Vector.Length];
            Array.Copy(item.Vector, ClusterVectorSummary, item.Vector.Length);
            ClusterItemList = new EstimatorCollection();
            ClusterItemList.Add(item);                                                                                                             
        }

        public bool RemoveItemFromCluster(EstimatorItem item)
        {
            if (ClusterItemList.Remove(item) == true)
            {
                if (ClusterItemList.Count > 0)  
                {
                    AdaptiveResonainceTheoryEstimator.CalculateIntersection(ClusterItemList, ClusterVector);
                    AdaptiveResonainceTheoryEstimator.CalculateSummary(ClusterItemList, ClusterVectorSummary);

                }
            }
            return ClusterItemList.Count > 0;
        }

        public void AddItemToCluster(EstimatorItem item)
        {
            if (!ClusterItemList.Contains(item))
            {
                ClusterItemList.Add(item);
                AdaptiveResonainceTheoryEstimator.UpdateIntersectionByLast(ClusterItemList, ClusterVector);
                AdaptiveResonainceTheoryEstimator.UpdateSummaryByLast(ClusterItemList, ClusterVectorSummary);
            }
        }
    }

}
