using System;
using UnityEngine;

namespace Pebble
{
    public class StageComponent<StageType, DefinitionType> : MonoBehaviour
                                                                            where StageType : Stage, new()
                                                                            where DefinitionType : StageDefinition
    {
        public DefinitionType ScreenDefinition;

        StageType stage = new StageType();

        public StageType Stage
        {
            get { return stage; }
        }

        // Use this for initialization
        void Start()
        {
            if (stage != null)
            {
                stage.Init(ScreenDefinition);
                stage.Start();
            }

        }

        // Update is called once per frame
        void Update()
        {
            if (stage != null)
            {
                stage.Update();
            }
        }

         void OnGUI()
         {
            if (stage != null)
            {
                stage.UpdateGUI();
            }
         }

        public virtual void OnDestroy()
        {
            if (stage != null)
            {
                stage.Stop();
                stage.Shutdown();
            }
        }
    }
}
