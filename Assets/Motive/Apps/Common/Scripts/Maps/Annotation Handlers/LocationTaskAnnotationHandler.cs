// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using System.Linq;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Handles adding/removing/selecting location task annotations.
    /// </summary>
    public class LocationTaskAnnotationHandler : SingletonTaskAnnotationHandler<LocationTaskAnnotationHandler, LocationTaskDriver>
    {
        protected override void ConfigureAnnotationObjectRange(MapAnnotation<LocationTaskDriver> annotation, AnnotationGameObject annotationObj)
        {
            if (annotationObj &&
                annotation.Data.Task.ActionRange != null)
            {
                annotationObj.RangeDisplayMode = AnnotationRangeDisplayMode.WhenSelected;

                var range = annotation.Data.Range;

                if (range != 0)
                {
                    if (range > 0)
                    {
                        // Normal range: you have to be "within" this range to
                        annotationObj.Range = (float)range;

                        annotationObj.InRangeColor = RangeCircleColor;
                        annotationObj.OutOfRangeColor = OutOfRangeColor;
                    }
                    else if (annotation.Data.Task.ActionRange.Min.HasValue)
                    {
                        // Inverse range: you have to be outside of this range
                        annotationObj.Range = (float)-range;

                        annotationObj.InRangeColor = InvertRangeCircleColor;
                        annotationObj.OutOfRangeColor = OutOfRangeColor;
                    }
                }
            }
        }

        public override AnnotationGameObject GetObjectForAnnotation(MapAnnotation<LocationTaskDriver> annotation)
        {
            AnnotationGameObject annotationObj = null;

            if (annotation.Data.Task.MarkerAssetInstance != null)
            {
                annotationObj = MapController.Instance.Create3DAssetAnnotation(annotation.Data.Task.MarkerAssetInstance);
            }
            else if (annotation.Data.Task.Marker != null)
            {
                annotationObj = MapMarkerAnnotationHandler.Instance.CreateMarkerAnnotation(annotation.Data.Task.Marker);
            }
            else if (UseTaskImageForAnnotation && annotation.Data.Task.ImageUrl != null)
            {
                var imgAnnot = Instantiate<CustomImageAnnotation>(MapMarkerAnnotationHandler.Instance.CustomImageAnnotation);

                annotationObj = imgAnnot;

                annotationObj.gameObject.SetActive(false);

                StartCoroutine(ImageLoader.LoadImage(annotation.Data.Task.ImageUrl, imgAnnot.gameObject, true, () =>
                {
                    annotationObj.gameObject.SetActive(true);
                }));
            }
            else
            {
                annotationObj = Instantiate<AnnotationGameObject>(GetPrefabForAnnotation(annotation));
            }

            base.ConfigureAnnotationObject(annotation, annotationObj);

            return annotationObj;
        }

        public void AddTaskAnnotations(LocationTaskDriver taskDriver)
        {
            AddTaskAnnotations(taskDriver, taskDriver.Task.Locations);
        }

        public override MapAnnotation<LocationTaskDriver> AddTaskAnnotation(LocationTaskDriver taskDriver, Location loc)
        {
            var ann = new MapAnnotation<LocationTaskDriver>(loc, taskDriver);

            ann.Marker = taskDriver.Task.Marker;
            ann.AssetInstance = taskDriver.Task.MarkerAssetInstance;

            return base.AddTaskAnnotation(taskDriver, loc);
        }

        internal void UpdateTaskAnnotations(LocationTaskDriver taskDriver)
        {
            UpdateTaskAnnotations(taskDriver, taskDriver.Task.Locations);
        }
    }

}