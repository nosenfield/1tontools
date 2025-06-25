using System;
using System.Collections.Generic;
using OneTon.Logging;

namespace OneTon.Utilities
{
    public class ObjectPool<T>
    {
        private static readonly LogService logger = LogService.Get<ObjectPool<T>>(LogLevel.Warn);
        private readonly Func<T> creationDelegate;
        Queue<T> pool;
        HashSet<T> hashSet;
        private int createdCount;
        public int CreatedCount => createdCount;
        private readonly object lockObj = new();

        public ObjectPool(Func<T> creationFunc)
        {
            pool = new();
            hashSet = new();
            creationDelegate = creationFunc;
        }

        public T GetObject()
        {
            lock (lockObj)
            {
                if (pool.Count > 0)
                {
                    T item = pool.Dequeue();
                    hashSet.Remove(item);
                    return item;
                }
            }

            createdCount++;
            return creationDelegate();
        }

        /// <summary>
        /// This function does not enforce any definition of the word "free".
        /// We do not keep track of every instance we make.
        /// Only the ones that are returned to us.
        /// </summary>
        /// <param name="freeObject"></param>
        public void AddObjectToPool(T freeObject)
        {
            lock (lockObj)
            {
                if (hashSet.Add(freeObject))
                {
                    pool.Enqueue(freeObject);
                }
                else
                {
                    logger.Warn($"Object is already in the pool");
                }
            }
        }

        public Queue<T> EmptyPool()
        {
            Queue<T> emptiedInstances = new(pool);
            pool.Clear();
            hashSet.Clear();
            return emptiedInstances;
        }
    }
}