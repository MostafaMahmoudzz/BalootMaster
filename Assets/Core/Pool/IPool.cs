using System;

namespace Pebble
{
    public interface IPool
    {
        void Free(IPoolable poolable);
    }
}
