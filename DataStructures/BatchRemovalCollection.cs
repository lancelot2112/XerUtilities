using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XerUtilities.DataStructures
{
    public class BatchRemovalCollection<T> : List<T>
    {
        private List<T> pendingRemovals;

        public BatchRemovalCollection()
        {
            pendingRemovals = new List<T>();
        }

        public void QueuePendingRemoval(T item)
        {
            pendingRemovals.Add(item);
        }

        public void ApplyPendingRemovals()
        {
            for (int i = 0; i < pendingRemovals.Count; i++)
            {
                Remove(pendingRemovals[i]);
            }
            pendingRemovals.Clear();
        }
    }
}
