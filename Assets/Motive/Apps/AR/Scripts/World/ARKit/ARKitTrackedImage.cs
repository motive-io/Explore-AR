using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

namespace Motive.Unity.AR
{
    public class ARKitTrackedImage : MonoBehaviour
    {
#if MOTIVE_ARKIT
        public ARImageAnchor ImageAnchor;
        public string Identifier;
        /*
        private void Update()
        {
            //transform.localScale = new Vector3(Image.ExtentX, 1, Image.ExtentZ);
        }*/
#endif
    }
}