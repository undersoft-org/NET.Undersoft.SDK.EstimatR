using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EstimatR
{
    public class EstimatorCollection : KeyedCollection<long, EstimatorItem>
    {
        public EstimatorCollection()
        {

        }

        public EstimatorCollection(IList<EstimatorItem> range) 
        {
           
        }

        public void AddRange(IEnumerable<EstimatorItem> range)
        {
            foreach (EstimatorItem de in range)
            {
                this.Add(de);
            }
        }

        protected override long GetKeyForItem(EstimatorItem item)
        {
            return item.Id;
        }
    }
}
