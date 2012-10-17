using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.Concurrent;
using System.Threading;

namespace XerUtilities.DataStructures.Parallel
{
    public class ConcurrentSerialQueue<T> : IProducerConsumerCollection<T>
    {
        private readonly object _syncLock;
        private readonly Queue<T> _queue;

        public ConcurrentSerialQueue()
        {
            _syncLock = new object();
            _queue = new Queue<T>();
        }

        public bool TryAdd(T item)
        {
            lock (_syncLock)
            {
                _queue.Enqueue(item);
                Monitor.Pulse(_syncLock);
            }
            return true;
        }

        public bool TryTake(out T item)
        {
            lock(_syncLock)
            {
                if (_queue.Count > 0)
                {
                    item = _queue.Dequeue();
                    return true;
                }
            }
            item = default(T);
            return false;
        }

        public int Count
        {
            get { lock (_syncLock) return _queue.Count; }
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        public object SyncRoot
        {
            get { return _syncLock; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int index)
        {
            throw new NotImplementedException();
        }

        public T[] ToArray()
        {
            throw new NotImplementedException();
        }
    }
}
