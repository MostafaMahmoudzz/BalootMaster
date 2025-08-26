using System;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------
//--------------------------------------------------------
// GameObjectEventDispatcher
//--------------------------------------------------------
//--------------------------------------------------------
namespace Pebble
{
    public class GameObjectEventDispatcher : MonoBehaviour
    {
        private EventDispatcher m_dispatcher;

        public GameObjectEventDispatcher()
        {
            m_dispatcher = new EventDispatcher();
        }

          //--------------------------------------------------------
        public static GameObjectEventDispatcher Get(GameObject gameObject)
        {
            if (gameObject != null)
            {
                GameObjectEventDispatcher dispatcher = gameObject.GetComponent<GameObjectEventDispatcher>();
                if(dispatcher == null)
                {
                    dispatcher = gameObject.AddComponent<GameObjectEventDispatcher>();    
                }
                return dispatcher;
            }
            return null;
        }

         //--------------------------------------------------------
        public static void Subscribe<T>(GameObject gameObject, EventDelegate<T> callback, EventChannel channel = EventChannel.Regular) where T : Event
        {
            GameObjectEventDispatcher dispatcher = Get(gameObject);
            if(dispatcher == null)
            {
                dispatcher.Subscribe<T>(callback, channel);
            }
        }

        //--------------------------------------------------------
        public static void UnSubscribe<T>(GameObject gameObject, EventDelegate<T> callback, EventChannel channel = EventChannel.Regular) where T : Event
        {
            GameObjectEventDispatcher dispatcher = Get(gameObject);
            if(dispatcher == null)
            {
                dispatcher.UnSubscribe<T>(callback, channel);
            }
        }

        //--------------------------------------------------------
        // Automatically free the event if inherit from PooledEvent
        public static void SendEvent<T>(GameObject gameObject, T evt) where T : Event
        {
            GameObjectEventDispatcher dispatcher = Get(gameObject);
            if(dispatcher == null)
            {
                dispatcher.SendEvent<T>(evt);
            }
        }

        public static void CreateAndSendPooledEvent<T>(GameObject gameObject, Action<T> InitFunc = null) where T : PooledEvent, new()
        {
            GameObjectEventDispatcher dispatcher = Get(gameObject);
            if(dispatcher == null)
            {
                dispatcher.CreateAndSendPooledEvent<T>(InitFunc);
            }
        }

        //--------------------------------------------------------
        // Automatically free the event if inherited from PooledEvent
        public static void CreateAndSendEvent<T>(GameObject gameObject, Action<T> InitFunc = null) where T : Event, new()
        {
            GameObjectEventDispatcher dispatcher = Get(gameObject);
            if(dispatcher == null)
            {
                dispatcher.CreateAndSendEvent<T>(InitFunc);
            }
        }

        //--------------------------------------------------------
        public void Subscribe<T>(EventDelegate<T> callback, EventChannel channel = EventChannel.Regular) where T : Event
        {
            m_dispatcher.Subscribe<T>(callback, channel);
        }

        //--------------------------------------------------------
        public void UnSubscribe<T>(EventDelegate<T> callback, EventChannel channel = EventChannel.Regular) where T : Event
        {
            m_dispatcher.UnSubscribe<T>(callback, channel);
         }

        //--------------------------------------------------------
        // Automatically free the event if inherit from PooledEvent
        public void SendEvent<T>(T evt) where T : Event
        {
            m_dispatcher.SendEvent<T>(evt);
        }

         //--------------------------------------------------------
        // Automatically free the event if inherited from PooledEvent
        public void CreateAndSendPooledEvent<T>(Action<T> InitFunc = null) where T : PooledEvent, new()
        {
            T evt = Pools.Claim<T>();
            if(InitFunc != null)
            {
                InitFunc(evt);
            }
            SendEvent<T>(evt);
        }

        //--------------------------------------------------------
        // Automatically free the event if inherited from PooledEvent
        public void CreateAndSendEvent<T>(Action<T> InitFunc = null) where T : Event, new()
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

