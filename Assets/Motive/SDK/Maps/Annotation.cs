// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.AR.LocationServices;
using UnityEngine.EventSystems;
using Motive.Core.Utilities;
using Motive.UI;
using Motive.AR.Models;
using System;
using Motive._3D.Models;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Represents an item on the map.
    /// </summary>
    public class MapAnnotation : IAnnotation
    {
        public Location Location { get; private set; }
        
        //public LocationTaskDriver LocationTaskDriver { get; set; }
        //public LocationMarker LocationMarker { get; set; }

        public MediaElement Marker { get; set; }
        public AssetInstance AssetInstance { get; set; }

        public IMapAnnotationDelegate Delegate { get; set; }

        private object m_data;

        public virtual object GetData()
        {
            return null;
        }

        public MapAnnotation(Location location, object data)
        {
            Location = location;
            m_data = data;
        }

        public MapAnnotation(Location location)
            : this (location, null)
        {
        }

        public Motive.AR.LocationServices.Coordinates Coordinates
        {
            get { return Location.Coordinates; }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            MapController.Instance.SelectAnnotation(this);
        }
    }

    public class MapAnnotation<T> : MapAnnotation
    {
        public T Data { get; private set; }

        public override object GetData()
        {
            return Data;
        }

        public MapAnnotation(Location location, T data)
            : base(location)
        {
            this.Data = data;
        }

        public MapAnnotation(Location location)
            : base(location)
        {

        }
    }

    public class UserAnnotation : IAnnotation
    {
        public Coordinates Coordinates
        {
            get; set;
        }

        public void UpdateCoordinates(Coordinates coords, float dt)
        {
            if (Coordinates == null)
            {
                Coordinates = coords;
            }
            else
            {
                Coordinates = Coordinates.Approach(coords, 0.1, dt);
            }
        }
    }
}
