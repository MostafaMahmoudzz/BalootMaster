using System;
using System.Collections.Generic;

namespace Pebble
{
    class Pools : Singleton<Pools>
    {
        private Dictionary<Type, IPool> m_pools;

        public Pools()
        {
            m_pools = new Dictionary<Type, IPool>();
        }

        //----------------------------------------------------------------------
        public static T Claim<T>() where T : class, IPoolable, new()
        {
            return Instance.OnClaim<T>();
        }

        //----------------------------------------------------------------------
        public static void Free(IPoolable obj)
        {
            Instance.OnFree(obj);
        }



        //----------------------------------------------------------------------
        private T OnClaim<T>() where T : class, IPoolable, new()
        {
            Type type = typeof(T);
            if (!m_pools.ContainsKey(type))
            {
                m_pools[type] = new Pool<T>();
            }
            return (m_pools[type] as Pool<T>).Claim();
        }

        //----------------------------------------------------------------------
        private void OnFree(IPoolable obj)
        {
            Type type = obj.GetType();
            if (m_pools.ContainsKey(type))
            {
                m_pools[type].Free(obj);
            }
        }
    }
}
