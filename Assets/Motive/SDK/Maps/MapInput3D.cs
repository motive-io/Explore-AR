// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Input for 3D maps.
    /// </summary>
    public class MapInput3D : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        Vector3 m_lastPos;
        bool m_pointerDown;

        public GameObject Swivel;
        public Vector3 SwivelAxis;
        public Camera Camera;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (m_pointerDown)
            {
                var camera = (Camera != null) ? Camera : Camera.main;

                if (!camera) return;

                var swivelCenter = camera.WorldToScreenPoint(Swivel.gameObject.transform.position);
                var pos = Input.mousePosition;

                var v1 = m_lastPos - swivelCenter;
                var v2 = pos - swivelCenter;

                var referenceRight = Vector3.Cross(Vector3.forward, v1);

                var angle = Vector2.Angle(v1, v2);

                float sign = Mathf.Sign(Vector3.Dot(v2, referenceRight));
                float finalAngle = sign * angle;

                m_lastPos = pos;

                Swivel.transform.Rotate(SwivelAxis, finalAngle);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_pointerDown = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_pointerDown = true;
            m_lastPos = Input.mousePosition;
        }
    }

}