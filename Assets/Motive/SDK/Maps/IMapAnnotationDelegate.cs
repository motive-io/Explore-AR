// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Attached to MapAnnotations to handle events from the MapController.
    /// </summary>
    public interface IMapAnnotationDelegate
    {
        AnnotationGameObject GetObjectForAnnotation(MapAnnotation annotation);
        void SelectAnnotation(MapAnnotation annotation);
        void DeselectAnnotation(MapAnnotation annotation);
    }
}
