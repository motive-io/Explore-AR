// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Default implementation of IMapAnnotationDelegate.
    /// </summary>
    public class MapAnnotationDelegate : IMapAnnotationDelegate
    {
        public Func<MapAnnotation, AnnotationGameObject> OnGetObjectForAnnotation { get; set; }
        public Action<MapAnnotation> OnSelect { get; set; }
        public Action<MapAnnotation> OnDeselect { get; set; }

        public AnnotationGameObject GetObjectForAnnotation(MapAnnotation annotation)
        {
            if (OnGetObjectForAnnotation != null)
            {
                return OnGetObjectForAnnotation(annotation);
            }

            return null;
        }

        public void SelectAnnotation(MapAnnotation annotation)
        {
            if (OnSelect != null)
            {
                OnSelect(annotation);
            }
        }

        public void DeselectAnnotation(MapAnnotation annotation)
        {
            if (OnDeselect != null)
            {
                OnDeselect(annotation);
            }
        }
    }
}
