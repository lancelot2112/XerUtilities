using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XerUtilities.DataStructures
{
    public class ObjectPool<T>: IDisposable 
        where T : IPooledObject , new()
    {
        Stack<T> objectStack;

        public ObjectPool(int initialCapacity)
        {
            objectStack = new Stack<T>((int)(1.25f*initialCapacity));

            for (int i = 0; i < initialCapacity; i++)
            {
                objectStack.Push(new T());
            }
        }

        public T GetNext()
        {
            if (objectStack.Count > 0)
            {
                return objectStack.Pop();
            }
            else
            {
                T obj = new T();
                obj.Initialize();
                return obj;
            }
        }

        public void Add(T obj)
        {
            obj.RemoveReferences();
            objectStack.Push(obj);
        }

        public void Dispose()
        {
            objectStack = null;
        }
    }

    public interface IPooledObject
    {
        bool Active { get; set; }
        void Initialize();
        void RemoveReferences();
    }
}
