// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.AR.LocationServices;
using Motive.AR.Scripting;
using System;
using System.Collections.Generic;
using Motive.Core.Utilities;
using Motive.Core.Timing;

namespace Motive.AR.LocationServices
{
    public class ForegroundPositionService : Singleton<ForegroundPositionService>
    {
        public LocationTracker LocationTracker { get; private set; }
        //public ILocationManager LocationManager { get { return m_unityLocationManager; } }
        public ICompass Compass { get { return Platform.Instance.Compass; } }

        public bool HasLocationData { get; private set; }

        public Coordinates Position { get; private set; }

        public event Action<Coordinates> PositionUpdated;

        HashSet<LocationFence> m_fences;
        //HashSet<LocationTrigger> m_triggers;

		Timer m_moveAnchorTimer;

        public void Initialize()
        {
            m_fences = new HashSet<LocationFence>();
            //m_triggers = new HashSet<LocationTrigger>();

			if (Platform.Instance.UseDeadReckoning)
			{
				var proc = new DeadReckoningProcessor(Platform.Instance.Pedometer, Compass);
                // Smooth location updates create a "spongy" feeling in the editor that gets
                // annoying really quickly
				proc.SmoothLocationUpdates = Platform.Instance.SmoothLocationUpdates && !Application.isEditor;

				UserLocationService.Instance.SystemReadingProcessor = proc;

				proc.Start();
			}

            LocationTracker = UserLocationService.Instance.CreateLocationTracker();
            LocationTracker.Updated += LocationTracker_Updated;

            LocationTracker.Start();

            Platform.Instance.OnEnterBackground.AddListener(() =>
            {
                LocationTracker.Stop();

                lock (m_fences)
                {
                    foreach (var fence in m_fences)
                    {
                        fence.Suspend();
                    }
                }
            });

            Platform.Instance.OnExitBackground.AddListener(() =>
            {
                LocationTracker.Start();

                lock (m_fences)
                {
                    foreach (var fence in m_fences)
                    {
                        fence.Resume();
                    }
                }
            });
        }

        void LocationTracker_Updated(object sender, LocationUpdaterEventArgs args)
        {
            //Debug.LogFormat("New coords: {0}", args.Reading.Coordinates.ToString());

            SetSystemPosition(args.Reading.Coordinates);
        }

        public void DebugSetHeading(float heading)
        {
            UnityCompass compass = Platform.Instance.SystemCompass as UnityCompass;

            if (compass)
            {
                compass.DebugSetHeading(heading);
            }
        }

        public void DebugSetPosition(Coordinates coords)
        {
            if (Application.isEditor)
            {
                ((UnityLocationManager)Platform.Instance.LocationManager).DebugSetPosition(coords);

                if (LocationTracker == null)
                {
                    // If we don't have a location tracker yet, 
                    // update the system position directly.
                    SetSystemPosition(coords);
                }
            }
        }

        public void SetAnchorPosition(Coordinates coords)
        {
			if (m_moveAnchorTimer != null)
			{
				m_moveAnchorTimer.Cancel();
				m_moveAnchorTimer = null;
			}

            UserLocationService.Instance.AnchorPosition = coords;
        }

		public void MoveToAnchorPosition(Coordinates coords, double speed)
		{
			var startCoords = UserLocationService.Instance.LastReading.Coordinates;

			var dist = startCoords.GetDistanceFrom(coords);

			var dt = dist / speed;

			MoveToAnchorPosition(coords, TimeSpan.FromSeconds(dt));
		}

		public void MoveToAnchorPosition(Coordinates coords, TimeSpan duration)
		{
			if (m_moveAnchorTimer != null)
			{
				m_moveAnchorTimer.Cancel();
				m_moveAnchorTimer = null;
			}

			var startTime = DateTime.Now;
			var startCoords = UserLocationService.Instance.LastReading.Coordinates;

			Debug.LogFormat("Warping from {0} to {1}",
				startCoords, coords);

			m_moveAnchorTimer = new Timer(0.5, () =>
				{
					var dt = DateTime.Now - startTime;

					Debug.LogFormat("Warping dt={0}", dt);

					try 
					{
						if (dt > duration)
						{
							Debug.LogFormat("Warping complete!");

							SetAnchorPosition(coords);
							m_moveAnchorTimer.Cancel();
						}
						else
						{
							var pct = dt.TotalSeconds / duration.TotalSeconds;

							var nextCoords = startCoords.Interpolate(coords, pct);

							Debug.LogFormat("Warping pct={0} next={1}", pct, nextCoords);

							UserLocationService.Instance.AnchorPosition = nextCoords;
						}
					}
					catch (Exception x)
					{
						Debug.LogException(x);
					}
				},
				true);

			m_moveAnchorTimer.Start();
		}

        private void SetSystemPosition(Coordinates coords)
        {
            if (coords != null)
            {
                Position = coords;

                HasLocationData = true;

                if (PositionUpdated != null)
                {
                    PositionUpdated(coords);
                }
            }
        }

        public LocationFence CreateFence(
            IEnumerable<Location> locations,
            double minRange,
            double maxRange,
            Action<IEnumerable<Location>> inRange,
            Action outOfRange)
        {
            var fence = LocationFence.Watch(
                locations,
                minRange,
                maxRange,
                inRange,
                outOfRange);

            lock (m_fences)
            {
                m_fences.Add(fence);
            }

            return fence;
        }

        public LocationFence CreateFence(
            Location location,
            double minRange,
            double maxRange,
            Action<IEnumerable<Location>> inRange,
            Action outOfRange)
        {
            var fence = LocationFence.Watch(
                location,
                minRange,
                maxRange,
                inRange,
                outOfRange);

            lock (m_fences)
            {
                m_fences.Add(fence);
            }

            return fence;
        }

        internal void DiscardFence(LocationFence fence)
        {
            fence.StopWatching();

            lock (m_fences)
            {
                m_fences.Remove(fence);
            }
        }
    }
}