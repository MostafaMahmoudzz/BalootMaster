using System;
using UnityEngine;

//-------------------------------------------------------
//-------------------------------------------------------
// Action
//-------------------------------------------------------
//-------------------------------------------------------
namespace Pebble
{
    public abstract class Action : IPoolable
    {

        enum State
        {
            Pending,
            Processing,
            Processed,
        }

        //----------------------------------------------
        // Variables
        private State m_state;
        private ActionQueue m_queue;

        //----------------------------------------------
        // Properties
        public bool IsPending
        {
            get { return m_state == State.Pending; }
        }

        public bool IsProcessed
        {
            get { return m_state == State.Processed; }
        }

        public ActionQueue Queue
        {
            get { return m_queue; }
        }

        //----------------------------------------------
        // Methods
        //-------------------------------------------------------
        public Action()
        {
            m_state = State.Pending;
        }

        //-------------------------------------------------------
        public void SetQueue(ActionQueue queue)
        {
            m_queue = queue;
        }

        //-------------------------------------------------------
        public void Process()
        {
            if (m_state == State.Pending)
            {
                m_state = State.Processing;
                OnStart();
            }
            else if (m_state == State.Processing)
            {
                OnProcess();
            }
        }

        protected void SetProcessed()
        {
            m_state = State.Processed;
        }

        public void Reset()
        {
            m_state = State.Pending;
            m_queue = null;
            OnReset();
        }

        protected abstract void OnReset();

        protected virtual void OnStart() { }
        protected virtual void OnProcess() { }
    }
}

