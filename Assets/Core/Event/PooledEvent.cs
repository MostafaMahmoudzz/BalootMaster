using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//---------------------------------------------------------------
//---------------------------------------------------------------
// Event that is pooled
//---------------------------------------------------------------
//---------------------------------------------------------------
namespace Pebble
{
    public abstract class PooledEvent : Event, IPoolable
    {
        public abstract void Reset();
    }
}

