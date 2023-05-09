namespace EstimatR
{

    public class HyperCluster
    {
        public double[] HyperClusterVector { get; set; }

        public List<Cluster> ClusterList { get; set; }

        public EstimatorCollection HyperClusterItemList { get; set; } 

        public double[] HyperClusterVectorSummary { get; set; }

        public HyperCluster(Cluster cluster)
        {
            HyperClusterVector = new double[cluster.ClusterVector.Length];
            Array.Copy(cluster.ClusterVector, HyperClusterVector, cluster.ClusterVector.Length);
            HyperClusterVectorSummary = new double[cluster.ClusterVectorSummary.Length];
            Array.Copy(cluster.ClusterVectorSummary, HyperClusterVectorSummary, cluster.ClusterVectorSummary.Length);
            ClusterList = new List<Cluster>();
            ClusterList.Add(cluster);
        }

        public bool RemoveClusterFromHyperCluster(Cluster cluster)
        {
            if (ClusterList.Remove(cluster) == true)
            {
                if (ClusterList.Count > 0)
                {
                    AdaptiveResonainceTheoryEstimator.CalculateClusterIntersection(ClusterList, HyperClusterVector);
                    AdaptiveResonainceTheoryEstimator.CalculateClusterSummary(ClusterList, HyperClusterVectorSummary);
                }
            }
            return ClusterList.Count > 0;
        }

        public void AddClusterToHyperCluster(Cluster cluster)
        {
            ClusterList.Add(cluster);
            AdaptiveResonainceTheoryEstimator.UpdateClusterIntersectionByLast(ClusterList, HyperClusterVector);
            AdaptiveResonainceTheoryEstimator.UpdateClusterSummaryByLast(ClusterList, HyperClusterVectorSummary);
        }


        public EstimatorCollection GetHyperClusterItemList()
        {
            EstimatorCollection updatedItemList = new EstimatorCollection();

            for (int i = 0; i < ClusterList.Count; i++)
            {
                for (int j = 0; j < ClusterList[i].ClusterItemList.Count; j++)
                {
                    updatedItemList.Add(ClusterList[i].ClusterItemList[j]);
                }
            }
            HyperClusterItemList = updatedItemList;

            return HyperClusterItemList;
        }

    }

}
