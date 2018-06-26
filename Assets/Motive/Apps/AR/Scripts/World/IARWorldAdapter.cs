// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Motive.Unity.AR
{
    public interface IARWorldAdapter
    {
        //Camera GetCamera();

        void Deactivate();

        void Activate();
    }

    public interface IARWorldAdapter<T> : IARWorldAdapter
        where T : ARWorldObject
    {
        Camera GetCameraForObject(T worldObj);
        void RemoveWorldObject(T worldObject);
        void AddWorldObject(T worldObject);
        double GetDistance(T worldObject);
    }
}
