using System;
using System.Collections.Generic;

namespace Pebble
{
    // Simple Pool class for Non-GameObjects classes
    class Pool<T> : IPool where T : class, IPoolable, new()
    {
        private List<T> m_freeObjects;
        private int m_peakCount;

        public int PeakCount
        {
            get
            {
                return m_peakCount;
            }
        }

        //----------------------------------------------------------
        public Pool(int initialCapacity)
        {
            m_freeObjects = new List<T>(initialCapacity);
            m_peakCount = 0;
        }

        //----------------------------------------------------------
        public Pool() : this(16)
        {

        }

        //----------------------------------------------------------
        public T Claim()
        {
            if (m_freeObjects.Count > 0)
            {
                return m_freeObjects.PopLast();
            }
            else
            {
                return new T();
            }
        }

        public void Free(T obj)
        {
            if (obj != null)
            {
                obj.Reset();
                m_freeObjects.Add(obj);
                m_peakCount = Math.Max(m_peakCount, m_freeObjects.Count);
            }
        }

        public void Clear()
        {
            m_freeObjects.Clear();
        }

        public void Free(IPoolable poolable)
        {
            T obj = poolable as T;
            Free(obj);
        }
    }
}
