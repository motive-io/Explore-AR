// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.AR.Vuforia;
using Motive.Core.Json;
using Motive.Core.Scripting;
using Motive.Unity.Models;

namespace Motive.Unity.Scripting
{
    /// <summary>
    /// Attaches AR features to the Motive scripting engine.
    /// </summary>
    public static class ARScriptExtensions
    {
        public static void Initialize()
        {
            JsonTypeRegistry.Instance.RegisterType("motive.ar.arTask", typeof(ARTask));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.augmented3dAsset", typeof(Augmented3DAsset));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.augmentedMedia", typeof(AugmentedMedia));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.locationAugmentedMedia", typeof(LocationAugmentedMedia));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.locationAugmentedImage", typeof(LocationAugmentedImage));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.locationAugmented3dAsset", typeof(LocationAugmented3DAsset));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.linearDistanceVariation", typeof(LinearDistanceVariation));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.fixedDistanceVariation", typeof(FixedDistanceVariation));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.locationValuablesCollection", typeof(LocationValuablesCollection));

            JsonTypeRegistry.Instance.RegisterType("motive.ar.visualMarkerMedia", typeof(VisualMarkerMedia));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.visualMarker3dAsset", typeof(VisualMarker3DAsset));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.visualMarkerTrackingCondition", typeof(VisualMarkerTrackingCondition));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.vuforia.frameMarker", typeof(FrameMarker)); // deprecated
            JsonTypeRegistry.Instance.RegisterType("motive.vuforia.vuMark", typeof(VuMark));
            JsonTypeRegistry.Instance.RegisterType("motive.vuforia.imageTarget", typeof(ImageTarget));

            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.arTask", new ARTaskProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.augmented3dAsset", new Augmented3DAssetProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.augmentedMedia", new AugmentedMediaProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.locationAugmentedImage", new LocationAugmentedImageProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.locationAugmentedMedia", new LocationAugmentedMediaProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.locationAugmented3dAsset", new LocationAugmented3DAssetProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.visualMarkerMedia", new VisualMarkerMediaProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.visualMarker3dAsset", new VisualMarker3DAssetProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.locationValuablesCollection", new LocationValuablesCollectionProcessor());

            // Could live somewhere else eventually:
            JsonTypeRegistry.Instance.RegisterType("motive.unity.asset", typeof(UnityAsset));

#if MOTIVE_VUFORIA
        ScriptEngine.Instance.RegisterConditionMonitor(VuforiaWorld.Instance.TrackingConditionMonitor);
        ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.visualMarkerTask", new VisualMarkerTaskProcessor());
#endif
        }
    }
}