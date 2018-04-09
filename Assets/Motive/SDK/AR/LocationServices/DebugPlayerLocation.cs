// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Maps;
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.AR.LocationServices
{
    public class DebugPlayerLocation : SingletonComponent<DebugPlayerLocation>
    {
        [SerializeField]
        public UnityLocation PlayerLocation;

        public bool KeyboardMovesPlayer;
        public bool MoveRelativeToHeading;
        public float PlayerSpeed = 50f;
		public float StartHeading;

        public bool UseAnchorPosition = false;

        public MapController MapController;

        UnityLocation m_currOverride;

        // Use this for initialization
		public void Initialize()
        {
            if (PlayerLocation)
            {
                WarpTo(PlayerLocation);
            }

			ForegroundPositionService.Instance.DebugSetHeading(StartHeading);
        }

		public void WarpToMapCenter()
		{
			UserLocationService.Instance.AnchorPosition = MapController.Instance.MapView.CenterCoordinates;
		}

        void WarpTo(Coordinates coords)
        {
            if (!Application.isMobilePlatform)
            {
                ForegroundPositionService.Instance.DebugSetPosition(coords);
            }

#if DEBUG
            if (UseAnchorPosition)
            {
                UserLocationService.Instance.AnchorPosition = coords;
            }
#endif
        }

        void WarpTo(UnityLocation unityLoc)
        {
            m_currOverride = unityLoc;

			if (unityLoc)
			{
				WarpTo(unityLoc.Coordinates.ToCoordinates());
			}
        }

        // Update is called once per frame
        void Update()
        {
            if (!Application.isMobilePlatform)
            {
                if (PlayerLocation)
                {
                    if (m_currOverride != PlayerLocation)
                    {
                        WarpTo(PlayerLocation);

                        MapController.Instance.CenterMap();
                    }
                }
            }

            if (KeyboardMovesPlayer)
            {
                Vector2 keyMove = Vector2.zero;

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    keyMove.y += 1;
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    keyMove.x += 1;
                }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    keyMove.x -= 1;
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    keyMove.y -= 1;
                }

                if (keyMove.magnitude > 0 && ForegroundPositionService.Instance.HasLocationData)
                {
                    var rad = Mathf.Atan2(keyMove.x, keyMove.y);
                    var heading = rad * 180.0 / Mathf.PI;

                    if (MoveRelativeToHeading)
                    {
                        heading += ForegroundPositionService.Instance.Compass.TrueHeading;
                    }

                    var coords = Platform.Instance.LocationManager.LastReading.Coordinates;
                    var newCoords = coords.AddRadial(heading, PlayerSpeed * Time.deltaTime);
                    ForegroundPositionService.Instance.DebugSetPosition(newCoords);
                }
            }
        }
    }
}