using System;
using System.Collections.Generic;

//--------------------------------------------------------
//--------------------------------------------------------
// GameEventDispatcher
//--------------------------------------------------------
//--------------------------------------------------------
namespace Pebble
{
    public class GameEventDispatcher : Singleton<GameEventDispatcher>
    {
        private EventDispatcher m_dispatcher;

        public GameEventDispatcher()
        {
            m_dispatcher = new EventDispatcher();
        }

        //--------------------------------------------------------
        public static void Subscribe<T>(EventDelegate<T> callback, EventChannel channel = EventChannel.Regular) where T : Event
        {
            if (Instance != null)
            {
                Instance.m_dispatcher.Subscribe<T>(callback, channel);
            }
        }

        //--------------------------------------------------------
        public static void UnSubscribe<T>(EventDelegate<T> callback, EventChannel channel = EventChannel.Regular) where T : Event
        {
            if (Instance != null)
            {
                Instance.m_dispatcher.UnSubscribe<T>(callback, channel);
            }
        }

        //--------------------------------------------------------
        // Automatically free the event if inherited from PooledEvent
        public static void SendEvent<T>(T evt) where T : Event
        {
            if (Instance != null)
            {
                Instance.m_dispatcher.SendEvent<T>(evt);
            }
        }

        //--------------------------------------------------------
        // Automatically free the event if inherited from PooledEvent
        public static void CreateAndSendPooledEvent<T>(Action<T> InitFunc = null) where T : PooledEvent, new()
        {
            if (Instance != null)
            {
                T evt = Pools.Claim<T>();
                if(InitFunc != null)
                {
                    InitFunc(evt);
                }
                SendEvent<T>(evt);
            }
        }

        //--------------------------------------------------------
        // Automatically free the event if inherited from PooledEvent
        public static void CreateAndSendEvent<T>(Action<T> InitFunc = null) where T : Event, new()
        {
            if (Instance != null && InitFunc != null)
            {
                T evt = new T();
                 if(InitFunc != null)
                {
                    InitFunc(evt);
                }
                SendEvent<T>(evt);
            }
        }
    }
}

