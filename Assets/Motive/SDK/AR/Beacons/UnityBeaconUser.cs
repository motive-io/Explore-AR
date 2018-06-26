// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

namespace Motive.Unity.Beacons
{
    public class UnityBeaconUser : MonoBehaviour
    {

        public float Speed = 1.0f;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Vector3 delta = Vector3.zero;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                delta.y += 1;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                delta.y -= 1;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                delta.x += 1;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                delta.x -= 1;
            }

            transform.position += (delta * Speed * Time.deltaTime);
        }
    }

}