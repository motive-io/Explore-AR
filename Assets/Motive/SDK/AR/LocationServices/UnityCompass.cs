// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Motive.AR.LocationServices;
using Motive.Core.Utilities;

namespace Motive.AR.LocationServices
{
    public class UnityCompass : MonoBehaviour, ICompass
    {
        double m_editorHeading;
      
        public double MagneticHeading
        {
            get;
            private set;
        }

        public double TrueHeading
        {
            get;
            private set;
        }

        void Update()
        {
            if (Application.isEditor)
            {
                if (Input.GetKey(KeyCode.S))
                {
                    m_editorHeading += Time.deltaTime * 360f;
                }

                if (Input.GetKey(KeyCode.A))
                {
                    m_editorHeading -= Time.deltaTime * 360f;
                }

                if (Input.GetKey(KeyCode.W))
                {
                    m_editorHeading += Time.deltaTime * 360f / 5;
                }

                if (Input.GetKey(KeyCode.Q))
                {
                    m_editorHeading -= Time.deltaTime * 360f / 5;
                }

				DebugSetHeading(m_editorHeading);
            }
            else
            {

                this.MagneticHeading = Input.compass.magneticHeading;
                this.TrueHeading = Input.compass.trueHeading;
            }
        }

		public void DebugSetHeading(double heading)
		{
			#if UNITY_EDITOR
			m_editorHeading = MathHelper.GetDegreesInRange(heading);

			this.MagneticHeading = m_editorHeading;
			this.TrueHeading = m_editorHeading;
			#endif
		}

        void ICompass.Start()
        {
            Input.compass.enabled = true;
        }

        void ICompass.Stop()
        {
            Input.compass.enabled = false;
        }
    }
}