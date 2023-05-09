using System.Diagnostics;
using System.Series;
using System.Text;

namespace EstimatR
{
    public class AdaptiveResonainceTheoryEstimator
    {
        public List<string> NameList { get; set; }

        public int ItemSize { get; set; }

        public EstimatorCollection ItemList { get; set; }

        public List<Cluster> ClusterList { get; set; }

        public List<HyperCluster> HyperClusterList { get; set; }

        private Catalog<Cluster> ItemToClusterMap;

        private Dictionary<Cluster, HyperCluster> ClusterToHyperClusterMap;

        public double bValue = 0.2f;

        public double pValue = 0.6f;

        public double p2Value = 0.3f;

        public const int rangeLimit = 1;

        public int IterationLimit = 50;

        private string tempHardFileName = "surveyResults.art";

        public AdaptiveResonainceTheoryEstimator()
        {
            NameList = new List<string>();
            ItemList = new EstimatorCollection();
            ClusterList = new List<Cluster>();
            HyperClusterList = new List<HyperCluster>();
            ItemToClusterMap = new Catalog<Cluster>();
            ClusterToHyperClusterMap = new Dictionary<Cluster, HyperCluster>();

            LoadFile(tempHardFileName);
            ItemList = NormalizeItemList(ItemList);
            Create();
        }

        public void Create()
        {
            ClusterList.Clear();
            HyperClusterList.Clear();
            ItemToClusterMap.Clear();
            ClusterToHyperClusterMap.Clear();

            for (int i = 0; i < ItemList.Count; i++)
            {
                AssignCluster(ItemList[i]);
            }

            for (int i = 0; i < HyperClusterList.Count; i++)
            {
                HyperClusterList[i].GetHyperClusterItemList();
            }
        }

        public void Create(ICollection<EstimatorItem> itemCollection)
        {
            ItemList.AddRange(itemCollection);

            ClusterList.Clear();
            HyperClusterList.Clear();
            ItemToClusterMap.Clear();
            ClusterToHyperClusterMap.Clear();

            for (int i = 0; i < ItemList.Count; i++)
            {
                ItemList[i].Id = i;
                AssignCluster(ItemList[i]);
            }

            for (int i = 0; i < HyperClusterList.Count; i++)
            {
                HyperClusterList[i].GetHyperClusterItemList();
            }
        }

        public void Append(ICollection<EstimatorItem> itemCollection)
        {
            int currentCount = ItemList.Count;

            ItemList.AddRange(itemCollection);

            for (int i = currentCount; i < ItemList.Count; i++)
            {
                ItemList[i].Id = i;
                AssignCluster(ItemList[i]);
            }

            for (int i = 0; i < HyperClusterList.Count; i++)
            {
                HyperClusterList[i].GetHyperClusterItemList();
            }
        }

        public void Append(EstimatorItem item)
        {
            item.Id = ItemList.Count;
            ItemList.Add(item);
            AssignCluster(item);

            for (int i = 0; i < HyperClusterList.Count; i++)
            {
                HyperClusterList[i].GetHyperClusterItemList();
            }
        }

        public void AssignCluster(EstimatorItem item)
        {
            int iterationCounter = IterationLimit;
            bool isAssignementChanged = true;
            double itemVectorMagnitude = CalculateVectorMagnitude(item.Vector);

            while (isAssignementChanged && iterationCounter > 0)
            {
                isAssignementChanged = false;

                List<KeyValuePair<Cluster, double>> clusterToProximityList =
                    new List<KeyValuePair<Cluster, double>>();
                double proximityThreshold = itemVectorMagnitude / (bValue + rangeLimit * ItemSize);

                for (int i = 0; i < ClusterList.Count; i++)
                {
                    double clusterVectorMagnitude = CalculateVectorMagnitude(
                        ClusterList[i].ClusterVector
                    );
                    double proximity =
                        CaulculateVectorIntersectionMagnitude(
                            item.Vector,
                            ClusterList[i].ClusterVector
                        ) / (bValue + clusterVectorMagnitude);
                    if (proximity > proximityThreshold)
                    {
                        clusterToProximityList.Add(
                            new KeyValuePair<Cluster, double>(ClusterList[i], proximity)
                        );
                    }
                }

                if (clusterToProximityList.Count > 0)
                {
                    clusterToProximityList.Sort((x, y) => -1 * x.Value.CompareTo(y.Value));

                    for (int i = 0; i < clusterToProximityList.Count; i++)
                    {
                        Cluster newCluster = clusterToProximityList[i].Key;
                        double vigilance =
                            CaulculateVectorIntersectionMagnitude(
                                newCluster.ClusterVector,
                                item.Vector
                            ) / itemVectorMagnitude;
                        if (vigilance >= pValue)
                        {
                            if (ItemToClusterMap.ContainsKey(item.Id))
                            {
                                Cluster previousCluster = ItemToClusterMap[item.Id];
                                if (ReferenceEquals(newCluster, previousCluster))
                                    break;
                                if (previousCluster.RemoveItemFromCluster(item) == false)
                                {
                                    ClusterList.Remove(previousCluster);
                                }
                            }
                            newCluster.AddItemToCluster(item);
                            ItemToClusterMap[item.Id] = newCluster;
                            isAssignementChanged = true;
                            break;
                        }
                    }
                }

                if (ItemToClusterMap.ContainsKey(item.Id) == false)
                {
                    Cluster newCluster = new Cluster(item);
                    ClusterList.Add(newCluster);
                    ItemToClusterMap.Add(item.Id, newCluster);
                    isAssignementChanged = true;
                }

                iterationCounter--;
            }

            AssignHyperCluster();
        }

        public void AssignHyperCluster()
        {
            int iterationCounter = IterationLimit;
            bool isAssignementChanged = true;

            while (isAssignementChanged && iterationCounter > 0)
            {
                isAssignementChanged = false;
                for (int j = 0; j < ClusterList.Count; j++)
                {
                    List<KeyValuePair<HyperCluster, double>> hyperClusterToProximityList =
                        new List<KeyValuePair<HyperCluster, double>>();
                    Cluster cluster = ClusterList[j];
                    double clusterVectorMagnitude = CalculateVectorMagnitude(cluster.ClusterVector);
                    double proximityThreshold =
                        clusterVectorMagnitude / (bValue + rangeLimit * ItemSize);

                    for (int i = 0; i < HyperClusterList.Count; i++)
                    {
                        double hyperClusterVectorMagnitude = CalculateVectorMagnitude(
                            HyperClusterList[i].HyperClusterVector
                        );
                        double proximity =
                            CaulculateVectorIntersectionMagnitude(
                                cluster.ClusterVector,
                                HyperClusterList[i].HyperClusterVector
                            ) / (bValue + hyperClusterVectorMagnitude);
                        if (proximity > proximityThreshold)
                        {
                            hyperClusterToProximityList.Add(
                                new KeyValuePair<HyperCluster, double>(
                                    HyperClusterList[i],
                                    proximity
                                )
                            );
                        }
                    }

                    if (hyperClusterToProximityList.Count > 0)
                    {
                        hyperClusterToProximityList.Sort((x, y) => -1 * x.Value.CompareTo(y.Value));

                        for (int i = 0; i < hyperClusterToProximityList.Count; i++)
                        {
                            HyperCluster newHyperCluster = hyperClusterToProximityList[i].Key;
                            double vigilance =
                                CaulculateVectorIntersectionMagnitude(
                                    newHyperCluster.HyperClusterVector,
                                    cluster.ClusterVector
                                ) / clusterVectorMagnitude;
                            if (vigilance >= p2Value)
                            {
                                if (ClusterToHyperClusterMap.ContainsKey(cluster))
                                {
                                    HyperCluster previousHyperCluster = ClusterToHyperClusterMap[
                                        cluster
                                    ];
                                    if (ReferenceEquals(newHyperCluster, previousHyperCluster))
                                        break;
                                    if (
                                        previousHyperCluster.RemoveClusterFromHyperCluster(cluster)
                                        == false
                                    )
                                    {
                                        HyperClusterList.Remove(previousHyperCluster);
                                    }
                                }
                                newHyperCluster.AddClusterToHyperCluster(cluster);
                                ClusterToHyperClusterMap[cluster] = newHyperCluster;
                                isAssignementChanged = true;

                                break;
                            }
                        }
                    }

                    if (ClusterToHyperClusterMap.ContainsKey(cluster) == false)
                    {
                        HyperCluster newHyperCluster = new HyperCluster(cluster);
                        HyperClusterList.Add(newHyperCluster);
                        ClusterToHyperClusterMap.Add(cluster, newHyperCluster);
                        isAssignementChanged = true;
                    }
                }

                iterationCounter--;
            }
        }

        public EstimatorItem SimilarTo(EstimatorItem item)
        {
            StringBuilder outputText = new StringBuilder();
            double tempItemSimilarSum = 0;
            double itemSimilarSum = 0;
            EstimatorItem itemSimilar = null;
            Cluster cluster = null;

            ItemToClusterMap.TryGet(item.Id, out cluster);
            if (cluster == null) { }
            else
            {
                EstimatorCollection clusterItemList = cluster.ClusterItemList;
                for (int i = 0; i < clusterItemList.Count; i++)
                {
                    if (!ReferenceEquals(item, clusterItemList[i]))
                    {
                        tempItemSimilarSum =
                            CaulculateVectorIntersectionMagnitude(
                                item.Vector,
                                clusterItemList[i].Vector
                            ) / CalculateVectorMagnitude(clusterItemList[i].Vector);
                        if (itemSimilarSum == 0 || itemSimilarSum < tempItemSimilarSum)
                        {
                            itemSimilarSum = tempItemSimilarSum;
                            itemSimilar = clusterItemList[i];
                        }
                    }
                }

                if (itemSimilar != null)
                {
                    outputText.Append(
                        " Most similiar taste have item " + itemSimilar.Name + "\r\n\r\n"
                    );
                }
                else
                {
                    outputText.Append(" There is no similiar item " + item.Name + "\r\n\r\n");
                }
            }
            Debug.WriteLine(outputText.ToString());

            return itemSimilar;
        }

        public EstimatorItem SimilarInGroupsTo(EstimatorItem item)
        {
            StringBuilder outputText = new StringBuilder();
            double tempItemSimilarSum = 0;
            double itemSimilarSum = 0;
            EstimatorItem itemSimilar = null;
            Cluster cluster = null;

            ItemToClusterMap.TryGet(item.Id, out cluster);
            if (cluster == null) { }
            else
            {
                HyperCluster hyperCluster = ClusterToHyperClusterMap[cluster];
                EstimatorCollection hyperClusterItemList = hyperCluster.GetHyperClusterItemList();
                for (int i = 0; i < hyperClusterItemList.Count; i++)
                {
                    if (!ReferenceEquals(item, hyperClusterItemList[i]))
                    {
                        tempItemSimilarSum =
                            CaulculateVectorIntersectionMagnitude(
                                item.Vector,
                                hyperClusterItemList[i].Vector
                            ) / CalculateVectorMagnitude(hyperClusterItemList[i].Vector);
                        if (itemSimilarSum == 0 || itemSimilarSum < tempItemSimilarSum)
                        {
                            itemSimilarSum = tempItemSimilarSum;
                            itemSimilar = hyperClusterItemList[i];
                        }
                    }
                }

                if (itemSimilar != null)
                {
                    outputText.Append(
                        " Most similiar taste in hyper cluster have item "
                            + itemSimilar.Name
                            + "\r\n\r\n"
                    );
                }
                else
                {
                    outputText.Append(
                        " There is no simiilar item in hyper cluster " + item.Name + "\r\n\r\n"
                    );
                }
            }
            Debug.WriteLine(outputText.ToString());

            return itemSimilar;
        }

        public EstimatorItem SimilarInOtherGroupsTo(EstimatorItem item)
        {
            StringBuilder outputText = new StringBuilder();
            double tempItemSimilarSum = 0;
            double itemSimilarSum = 0;
            EstimatorItem itemSimilar = null;

            if (!ItemToClusterMap.TryGet(item.Id, out Cluster cluster)) { }
            else
            {
                HyperCluster hyperCluster = ClusterToHyperClusterMap[cluster];
                for (int j = 0; j < hyperCluster.ClusterList.Count; j++)
                {
                    if (!ReferenceEquals(cluster, hyperCluster.ClusterList[j]))
                    {
                        EstimatorCollection clusterItemList = hyperCluster.ClusterList[
                            j
                        ].ClusterItemList;
                        for (int i = 0; i < clusterItemList.Count; i++)
                        {
                            tempItemSimilarSum =
                                CaulculateVectorIntersectionMagnitude(
                                    item.Vector,
                                    clusterItemList[i].Vector
                                ) / CalculateVectorMagnitude(clusterItemList[i].Vector);
                            if (itemSimilarSum == 0 || itemSimilarSum < tempItemSimilarSum)
                            {
                                itemSimilarSum = tempItemSimilarSum;
                                itemSimilar = clusterItemList[i];
                            }
                        }
                    }
                }

                if (itemSimilar != null)
                {
                    outputText.Append(
                        " Most similiar taste in hyper cluster (other clusters) have item "
                            + itemSimilar.Name
                            + "\r\n\r\n"
                    );
                }
                else
                {
                    outputText.Append(
                        " There is no simiilar item in hyper cluster (other clusters) "
                            + item.Name
                            + "\r\n\r\n"
                    );
                }
            }
            Debug.WriteLine(outputText.ToString());

            return itemSimilar;
        }

        public static double[] CalculateIntersection(EstimatorCollection input, double[] output)
        {
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = input[0].Vector[i];
                for (int j = 1; j < input.Count; j++)
                {
                    output[i] = Math.Min(output[i], input[j].Vector[i]);
                }
            }
            return output;
        }

        public static double[] CalculateSummary(EstimatorCollection input, double[] output)
        {
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = 0;
                for (int j = 0; j < input.Count; j++)
                {
                    output[i] += input[j].Vector[i];
                }
            }

            return output;
        }

        public static double[] UpdateIntersectionByLast(EstimatorCollection input, double[] output)
        {
            int n = input.Count - 1;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = Math.Min(output[i], input[n].Vector[i]);
            }
            return output;
        }

        public static double[] UpdateSummaryByLast(EstimatorCollection input, double[] output)
        {
            int n = input.Count - 1;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] += input[n].Vector[i];
            }
            return output;
        }

        public static double[] CalculateClusterIntersection(List<Cluster> input, double[] output)
        {
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = input[0].ClusterVector[i];
                for (int j = 1; j < input.Count; j++)
                {
                    output[i] = Math.Min(output[i], input[j].ClusterVector[i]);
                }
            }
            return output;
        }

        public static double[] CalculateClusterSummary(List<Cluster> input, double[] output)
        {
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = 0;
                for (int j = 0; j < input.Count; j++)
                {
                    output[i] += input[j].ClusterVector[i];
                }
            }

            return output;
        }

        public static double[] UpdateClusterIntersectionByLast(List<Cluster> input, double[] output)
        {
            int n = input.Count - 1;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = Math.Min(output[i], input[n].ClusterVector[i]);
            }
            return output;
        }

        public static double[] UpdateClusterSummaryByLast(List<Cluster> input, double[] output)
        {
            int n = input.Count - 1;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] += input[n].ClusterVector[i];
            }
            return output;
        }

        public static EstimatorCollection NormalizeItemList(EstimatorCollection featureItemList)
        {
            EstimatorCollection normalizedItemList = new EstimatorCollection();

            int length;
            for (int i = 0; i < featureItemList.Count; i++)
            {
                length = featureItemList[0].Vector.Length;
                double[] featureVector = new double[length];
                for (int j = 0; j < length; j++)
                {
                    featureVector[j] = featureItemList[i].Vector[j] / 10.00;
                }
                normalizedItemList.Add(
                    new EstimatorItem(
                        featureItemList[i].Id,
                        (string)featureItemList[(int)i].Name,
                        featureVector
                    )
                );
            }
            return normalizedItemList;
        }

        static public double CalculateVectorMagnitude(double[] vector)
        {
            double result = 0;
            for (int i = 0; i < vector.Length; ++i)
            {
                result += vector[i];
            }
            return result;
        }

        static public double CaulculateVectorIntersectionMagnitude(
            double[] vector1,
            double[] vector2
        )
        {
            double result = 0;

            for (int i = 0; i < vector1.Length; ++i)
            {
                result += Math.Min(vector1[i], vector2[i]);
            }

            return result;
        }

        public void LoadFile(string fileLocation)
        {
            string line;
            NameList.Clear();
            NameList.Add("Name");

            StreamReader file = new StreamReader(fileLocation);

            while ((line = file.ReadLine()) != null)
            {
                if (line == "ItemList")
                {
                    break;
                }
            }

            if (line == null)
            {
                throw new Exception("ART File does not have a section marked ItemList!");
            }
            else
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (line == "--")
                    {
                        break;
                    }
                    else
                    {
                        NameList.Add(line);
                    }
                }
                ItemSize = NameList.Count - 1;

                int featureItemId = 0;
                while ((line = file.ReadLine()) != null)
                {
                    string featureName = line;
                    line = file.ReadLine();
                    double[] featureVector = new double[ItemSize];
                    int i = 0;
                    while ((line != null) && (line != "--"))
                    {
                        featureVector[i] = Int32.Parse(line);
                        ++i;
                        line = file.ReadLine();
                    }

                    if (line == "--")
                    {
                        if (i != ItemSize)
                        {
                            for (int j = i; j < ItemSize; ++j)
                            {
                                featureVector[j] = 0;
                            }
                        }
                        ItemList.Add(new EstimatorItem(featureItemId, featureName, featureVector));
                        featureItemId++;
                    }
                }
            }

            file.Close();
        }
    }
}
