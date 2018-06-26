// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Motive.Core.Utilities;
using UnityEngine.Events;

namespace Motive.Unity.Maps
{
    public class MapInputEventArgs : EventArgs
    {
        public Vector2 TouchPosition { get; private set; }

        public MapInputEventArgs(Vector2 mousePos)
        {
            TouchPosition = mousePos;
        }
    }

    public class MapInputEvent : UnityEvent<MapInputEventArgs>
    {
    }

    /// <summary>
    /// Calculate swipe and zoom for the map based on multi-touch.
    /// </summary>
    public class MapInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        private float m_lastPinchMagnitude;

        private bool m_pointerDown;

        public bool PointerDown { get { return m_pointerDown; } }

        private Vector2 m_lastTranslatePos;
        private Vector2 m_translateVelocity;
        private Vector2 m_translation;
        private float m_holdTime;
        private bool m_holding;
        private bool m_waitUnhold;
        private float m_lastClickTime;

        public float DoubleClickSpeed = 0.33f;
        public float TranslateDamp = 0.33f;
        public float TranslateEpsilon = 1.0f;
        public float ClickHoldDuration = 1.0f;
        public float HoldMaxTranslate = 1.0f;
        public bool AllowPinchZoom = true;
        public bool DoubleClickToZoom = true;
        public float DoubleClickZoomDelta = 0.75f;

        public MapInputEvent OnHold;
        public MapInputEvent OnDoubleClick;

        static double m_log2 = Math.Log(2);

        void Awake()
        {
            if (OnHold == null)
            {
                OnHold = new MapInputEvent();
            }

            if (OnDoubleClick == null)
            {
                OnDoubleClick = new MapInputEvent();
            }
        }

        // Use this for initialization
        void Start()
        {
#if UNITY_EDITOR
            Debug.Log("*****Press LEFT SHIFT to simulate a pinch between the mouse and the screen center.*****");
#endif
        }

        public bool IsTranslating
        {
            get; private set;
        }

        public Vector2 Translation
        {
            get
            {
                if (IsTranslating)
                {
                    return m_translation;
                }

                return new Vector2(0, 0);
            }
        }

        public bool IsPinching {
            get;
            private set; 
        }

        public double ZoomDelta
        {
            get; set;
        }

        Vector2 GetVTranslate()
        {
            Vector2 pos = Input.mousePosition;

            var trans = pos - m_lastTranslatePos;
            m_lastTranslatePos = pos;

            return trans / Time.smoothDeltaTime;
        }

        // Update is called once per frame
        void Update()
        {
            bool pinching = false;

            if (UnityEngine.Application.isMobilePlatform)
            {
                pinching = Input.touchCount > 1;
            }
            else
            {
                pinching = m_pointerDown && (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift));
            }

            if (AllowPinchZoom && pinching)
            {
                IsTranslating = false;

                Vector2 touch1Pos;
                Vector2 touch2Pos;

                if (UnityEngine.Application.isMobilePlatform)
                {
                    touch1Pos = Input.touches[0].position;
                    touch2Pos = Input.touches[1].position;
                }
                else
                {
                    touch1Pos = Input.mousePosition;
                    touch2Pos = new Vector2(Screen.width / 2, Screen.height / 2);
                }

                Vector2 currentPinch = (touch1Pos - touch2Pos);

                if (!IsPinching)
                {
					PinchStarted();
                }
                else
                {
                    var pinchFactor = (currentPinch.magnitude / m_lastPinchMagnitude);
                    ZoomDelta = Math.Log(pinchFactor) / m_log2;
                }

                m_lastPinchMagnitude = currentPinch.magnitude;
            }
            else
            {
                if (IsPinching)
                {
                    PinchEnded();
                }
            }

            if (!pinching && m_pointerDown)
            {
                if (!IsTranslating)
                {
                    m_lastTranslatePos = Input.mousePosition;
                }

                IsTranslating = !m_waitUnhold;

                var lastV = m_translateVelocity;
                var currV = GetVTranslate();

                m_translateVelocity = ((lastV + currV * 3) / 4);

                if (m_translateVelocity.magnitude < HoldMaxTranslate)
                {
                    if (m_holding)
                    {
                        if (Time.time - m_holdTime > ClickHoldDuration && !m_waitUnhold)
                        {
                            m_waitUnhold = true;
                            IsTranslating = false;

                            if (OnHold != null)
                            {
                                OnHold.Invoke(new MapInputEventArgs(Input.mousePosition));
                            }
                        }
                    }
                    else
                    {
                        m_holding = true;
                        m_holdTime = Time.time;
                    }
                }
                else
                {
                    m_holding = false;
                }

                if (IsTranslating)
                {
                    m_translation = m_translateVelocity * Time.smoothDeltaTime;
                }
            }
            else
            {
                m_holding = false;
                
                if (m_translateVelocity.sqrMagnitude > TranslateEpsilon)
                {
                    var vx = MathHelper.Approach(m_translateVelocity.x, 0, TranslateDamp, Time.deltaTime);
                    var vy = MathHelper.Approach(m_translateVelocity.y, 0, TranslateDamp, Time.deltaTime);

                    m_translateVelocity = new Vector2(vx, vy);

                    m_translation = m_translateVelocity * Time.deltaTime;
                }
                else
                {
                    IsTranslating = false;
                }
            }
        }

        public void CancelPan()
        {
            IsTranslating = false;
        }

		void PinchStarted()
		{
			IsPinching = true;
			m_lastClickTime = 0;
		}

        void PinchEnded()
        {
            IsPinching = false;
            ZoomDelta = 0;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Reset wait_unhold after pointer up
            m_waitUnhold = false;
            m_pointerDown = true;
            m_lastTranslatePos = Input.mousePosition;
            m_translateVelocity = Vector2.zero;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PinchEnded();
            // Reset wait_unhold after pointer up
            m_waitUnhold = false;
            m_pointerDown = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
			if (Time.time - m_lastClickTime < DoubleClickSpeed &&
				Input.touchCount < 2 &&
				!IsPinching)
			{
				if (OnDoubleClick != null)
				{
					OnDoubleClick.Invoke(new MapInputEventArgs(eventData.position));
				}
			}

			m_lastClickTime = Time.time;
        }
    }
}