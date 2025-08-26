using System.Collections.Generic;

//-------------------------------------------------------
//-------------------------------------------------------
// EffectQueue
//-------------------------------------------------------
//-------------------------------------------------------
namespace Pebble
{
    public class ActionQueue
    {
        //----------------------------------------------
        // Variables
        private List<Action> m_queue;

        //----------------------------------------------
        // Properties

        //----------------------------------------------
        // Methods
        //-------------------------------------------------------
        public ActionQueue()
        {
            m_queue = new List<Action>();
        }

        //-------------------------------------------------------
        public void AddAction(Action action)
        {
            if (action != null)
            {
                action.SetQueue(this);
                m_queue.Add(action);
            }
        }

        public void Process()
        {
            if (m_queue.Count == 0)
            {
                return;
            }

            Action currentAction = m_queue.Front();
            currentAction.Process();
            // No else here alllows for the action to be processed in the start
            if (currentAction.IsProcessed)
            {
                m_queue.PopFront();
                Pools.Free(currentAction);
            }
        }
    }
}

