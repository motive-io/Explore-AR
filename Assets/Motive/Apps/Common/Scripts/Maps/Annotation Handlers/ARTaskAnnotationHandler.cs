// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Gaming;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Manages map annotations for AR tasks.
    /// </summary>
    public class ARTaskAnnotationHandler : SingletonTaskAnnotationHandler<ARTaskAnnotationHandler, ARTaskDriver>
    {
        protected override void ConfigureAnnotationObjectRange(MapAnnotation<ARTaskDriver> annotation, AnnotationGameObject annotationObj)
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

        protected override bool ShouldSetPanelFence(ARTaskDriver driver, out Motive.Core.Models.DoubleRange range)
        {
            range = driver.Task.ActionRange;

            return range != null;
        }
    }

}