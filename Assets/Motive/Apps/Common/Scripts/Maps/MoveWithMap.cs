// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using UnityEngine;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Attach to a game object to make it move with the map. Can also be used to
    /// create static elements that replace a map.
    /// </summary>
    public class MoveWithMap : MonoBehaviour
    {
        public float CenterLatitude;
        public float CenterLongitude;
        public float MaxDistance;
        public float MetersPerUnit;

        Vector3 m_startPos;
        Coordinates m_startCoords;

        void Start()
        {
            m_startPos = transform.localPosition;
            m_startCoords = new Coordinates(CenterLatitude, CenterLongitude);

            SetPosition();
        }

        void SetPosition()
        {
            var coords = ForegroundPositionService.Instance.Position;

            if (coords != null)
            {
                var d = (float)m_startCoords.GetDistanceFrom(coords);

                if (d > MaxDistance)
                {
                    d = 0;
                    return;
                }

                float dlon = (float)coords.Longitude - CenterLongitude;
                float dlat = (float)coords.Latitude - CenterLatitude;

                var r = Mathf.Atan2(dlat, dlon);

                var pos = m_startPos;

                var dx = d * Mathf.Cos(r) / MetersPerUnit;
                var dy = d * Mathf.Sin(r) / MetersPerUnit;

                pos.x -= dx;
                pos.y -= dy;

                transform.localPosition = pos;
            }
        }

        void Update()
        {
            SetPosition();
        }
    }

}