// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnityCoordinates
{
    public double Latitude;
    public double Longitude;

    public Coordinates ToCoordinates()
    {
        return new Coordinates(Latitude, Longitude);
    }
}
