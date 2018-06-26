// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Debug component for testing AR mode.
    /// </summary>
    public class ARHud : MonoBehaviour
    {
        public Text HeadingText;
        public Text Diag2;
        public Text Diag3;

        CameraGyroscope m_gyro;

        // Use this for initialization
        void Start()
        {
            m_gyro = GameObject.FindObjectOfType<CameraGyroscope>();

            //m_gyro.SetHeading((float)Platform.Instance.NoiseReducedCompass.TrueHeading);
        }

        // Update is called once per frame
        //   void Update () {
        //       HeadingText.text =
        //		string.Format("h={0:0} gy={1:0} dy={2:0} cy={3:0} ch={4:0} gz={5:0} d={6:0}", 
        //		Platform.Instance.NoiseReducedCompass.TrueHeading,
        //			m_gyro.transform.localEulerAngles.y,
        //			m_gyro.m_deviceSwivel.localEulerAngles.y,
        //			m_gyro.m_compassSwivel.localEulerAngles.y,
        //			m_gyro.camHdg,
        //			//m_gyro.transform.localEulerAngles.z,
        //			Input.gyro.attitude.eulerAngles.z,
        //			m_gyro.hdgDiff);

        //	Diag2.text = 
        //		string.Format("gyro={0}",
        //			Input.gyro.attitude.eulerAngles.ToString());

        //       Diag3.text =
        //           string.Format("h={0:0} c={1:0} tilt={2:0}",
        //               m_gyro.camHdg,
        //               m_gyro.camCompositeHdg,
        //               m_gyro.camTilt);

        //	//HeadingText.text = string.Format("{0:0},{1:0},{2:0}",
        //       //    Input.gyro.attitude.eulerAngles.x,
        //       //    Input.gyro.attitude.eulerAngles.y,
        //       //    Input.gyro.attitude.eulerAngles.z);

        //	//Calibrate();
        //}

        public void Calibrate()
        {
            m_gyro.SetHeading((float)Platform.Instance.NoiseReducedCompass.TrueHeading);
        }
    }

}