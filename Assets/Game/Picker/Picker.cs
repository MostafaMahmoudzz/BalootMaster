using System;
using UnityEngine;

//----------------------------------------------------
//----------------------------------------------------
// Picker
//----------------------------------------------------
//----------------------------------------------------

namespace Pebble
{
    public class Picker : Singleton<Picker>
    {
        //----------------------------------------------
        // Variables
        public Vector3 m_mouseWorldPoint;
        public GameObject m_underMouse;
        public RaycastHit m_raycastHit;
        private bool m_computedUnderMouseThisFrame;

        //----------------------------------------------
        // Properties
        public GameObject UnderMouse
        {
            get
            {
                return GetObjectUnderMouse();
            }
        }

        public Vector3 MouseWorldPos
        {
            get
            {
                Compute();
                return m_mouseWorldPoint;
            }
        }


        //----------------------------------------------
        public Picker()
        {
            m_raycastHit = new RaycastHit();
            m_mouseWorldPoint = new Vector3();
            m_computedUnderMouseThisFrame = false;
        }

        //----------------------------------------------
        public override void Awake()
        {
            base.Awake();
        }

        //----------------------------------------------
        void Start()
        {

        }

        //----------------------------------------------
        void Update()
        {
            m_computedUnderMouseThisFrame = false;
        }

        //----------------------------------------------
        protected GameObject GetObjectUnderMouse()
        {
            Compute();
            return m_underMouse;
        }

        protected void Compute()
        {
            if (!m_computedUnderMouseThisFrame)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out m_raycastHit))
                {
                    m_underMouse = m_raycastHit.collider.gameObject;
                }
                else
                {
                    m_underMouse = null;
                }

                float t = -ray.origin.z / ray.direction.z;
                m_mouseWorldPoint = ray.GetPoint(t);
                m_computedUnderMouseThisFrame = true;
            }
        }
    }
}
