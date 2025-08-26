using System;
using System.Collections.Generic;

//--------------------------------------------------------
//--------------------------------------------------------
// EventDispatcher
//--------------------------------------------------------
//--------------------------------------------------------
namespace Pebble
{
    public class EventDispatcher
    {
        private Dictionary<Type, Delegate>[] m_listeners;

        public EventDispatcher()
        {
            m_listeners = new Dictionary<Type, Delegate>[(int)EventChannel.Count];

            for (int channel = 0; channel < (int)EventChannel.Count; ++channel)
            {
                m_listeners[channel] = new Dictionary<Type, Delegate>();
            }
        }

        //--------------------------------------------------------
        public void Subscribe<T>(EventDelegate<T> callback, EventChannel channel) where T : Event
        {
            if (channel == EventChannel.Count)
                return;

            int intChannel = (int)channel;
            Type type = typeof(T);
            Delegate existingDelegate;
            if (m_listeners[intChannel].TryGetValue(type, out existingDelegate))
            {
                EventDelegate<T> existingEventDelegate = existingDelegate as EventDelegate<T>;
                existingEventDelegate += callback;
                m_listeners[intChannel][type] = existingEventDelegate;
            }
            else
            {
                m_listeners[intChannel][type] = callback;
            }
        }

        //--------------------------------------------------------
        public void UnSubscribe<T>(EventDelegate<T> callback, EventChannel channel) where T : Event
        {
            if (channel == EventChannel.Count)
                return;

            int intChannel = (int)channel;
            Type type = typeof(T);
            Delegate existingDelegate;
            if (m_listeners[intChannel].TryGetValue(type, out existingDelegate))
            {
                EventDelegate<T> existingEventDelegate = existingDelegate as EventDelegate<T>;
                existingEventDelegate -= callback;
                if (existingEventDelegate == null)
                {
                    m_listeners[intChannel].Remove(type);
                }
                else
                {
                    m_listeners[intChannel][type] = existingEventDelegate;
                }
            }
        }

        //--------------------------------------------------------
        public void SendEvent<T>(T evt) where T : Event
        {
            Type type = evt.GetType();
            Delegate existingDelegate;

            for (int channel = 0; channel < (int)EventChannel.Count; ++channel)
            {
                if (m_listeners[channel].TryGetValue(type, out existingDelegate))
                {
                    EventDelegate<T> existingEventDelegate = existingDelegate as EventDelegate<T>;
                    if (existingEventDelegate != null)
                    {
                        existingEventDelegate(evt);
                    }
                    /*
                    else
                    {
                        existingDelegate.DynamicInvoke(evt);
                    }  */
                }
            }

            PooledEvent pooledEvent = evt as PooledEvent;
            if (pooledEvent != null)
            {
                Pools.Free(pooledEvent);
            }
        }
    }
}

