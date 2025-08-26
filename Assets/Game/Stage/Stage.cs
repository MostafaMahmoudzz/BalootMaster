using System;

//----------------------------------------------
//----------------------------------------------
// Stage
//----------------------------------------------
//----------------------------------------------

namespace Pebble
{
    public abstract class Stage
    {
        //----------------------------------------------
        // Variables

        private StageDefinition m_definition;

        //----------------------------------------------
        // Properties
        public StageDefinition Definition
        {
            /* You can override this property in your base class for easy access : 

            public new MyDefinition Definition
            {
                get 
                { 
                    return base.Definition as MyDefinition;
                }
            } */
            get { return m_definition; }
        }

        //----------------------------------------------
        // Methods

        public void Init(StageDefinition definition)
        {
            m_definition = definition;

            OnInit();
        }


        public void Start()
        {
            OnStart();
        }

        public void Stop()
        {
            OnStop();
        }

        public void Shutdown()
        {
            OnShutdown();
        }

        // Update is called once per frame
        public void Update()
        {
            OnUpdate();
        }

        public void UpdateGUI()
        {
             OnGUI();   
        }

        protected abstract void OnInit();
        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract void OnShutdown();
        protected abstract void OnUpdate();
        protected abstract void OnGUI();

    }
}

