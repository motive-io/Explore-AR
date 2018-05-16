// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive._3D.Models;
using Motive.AR.Beacons;
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.AR.Scripting;
using Motive.Attractions.Models;
using Motive.Core.Json;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Google;
using Motive.UI;
using Motive.UI.Models;
using Motive.Unity.Models;
using Motive.Unity.Storage;

namespace Motive.Unity.Scripting
{
    /// <summary>
    /// Connects the Unity project to the Motive scripting engine.
    /// </summary>
    public static class ScriptExtensions
    {
        public static void Initialize(ILocationManager locationManager, IBeaconManager beaconManager, ICompass compass)
        {
            // This code bootstraps the Motive script engine. First, give the script engine
            // a path that it can use for storing state.
            var smPath = StorageManager.GetGameStorageManager().GetManager("scriptManager");

            ScriptEngine.Instance.Initialize(smPath);

            // This intializes the Alternate Reality and Location Based features.
            ARComponents.Instance.Initialize(smPath, locationManager, beaconManager, compass, null);

            // This tells the JSON reader how to deserialize various object types based on
            // the "type" field.
            JsonTypeRegistry.Instance.RegisterType("motive.core.scriptDirectoryItem", typeof(ScriptDirectoryItem));
            JsonTypeRegistry.Instance.RegisterType("motive.core.appVersionCondition", typeof(AppVersionCondition));

            JsonTypeRegistry.Instance.RegisterType("motive.ui.notification", typeof(Notification));

            JsonTypeRegistry.Instance.RegisterType("motive.ui.interfaceUpdate", typeof(InterfaceUpdate));
            JsonTypeRegistry.Instance.RegisterType("motive.ui.interfaceDirector", typeof(InterfaceDirector));
            JsonTypeRegistry.Instance.RegisterType("motive.ui.objectInspector", typeof(ObjectInspector));
            JsonTypeRegistry.Instance.RegisterType("motive.ui.uiModeSwitchCommand", typeof(UIModeSwitchCommand));

            JsonTypeRegistry.Instance.RegisterType("motive.3d.worldObjectEffectPlayer", typeof(WorldObjectEffectPlayer));
            JsonTypeRegistry.Instance.RegisterType("motive.3d.relativeWorldPosition", typeof(RelativeWorldPosition));
            JsonTypeRegistry.Instance.RegisterType("motive.3d.fixedWorldPosition", typeof(FixedWorldPosition));
            JsonTypeRegistry.Instance.RegisterType("motive.3d.worldObjectProximityCondition", typeof(WorldObjectProximityCondition));

            JsonTypeRegistry.Instance.RegisterType("motive.unity.animation", typeof(UnityAnimation));

            JsonTypeRegistry.Instance.RegisterType("motive.gaming.character", typeof(Character));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.collectible", typeof(Collectible));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.assignment", typeof(Assignment));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.taskObjectiveCompleteCondition", typeof(TaskObjectiveCompleteCondition));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.objectiveActivator", typeof(ObjectiveActivator));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.objectiveCompleter", typeof(ObjectiveCompleter));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.playerTask", typeof(PlayerTask));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.characterTask", typeof(CharacterTask));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.playableContent", typeof(PlayableContent));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.playableContentBatch", typeof(PlayableContentBatch));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.characterMessage", typeof(CharacterMessage));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.screenMessage", typeof(ScreenMessage));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.inventoryCollectibles", typeof(InventoryCollectibles));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.inventoryCollectibleLimit", typeof(InventoryCollectibleLimit));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.playerReward", typeof(PlayerReward));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.walletCurrency", typeof(WalletCurrency));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.walletCurrencyLimit", typeof(WalletCurrencyLimit));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.savePoint", typeof(SavePoint));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.interfaceAction", typeof(InterfaceAction));

            JsonTypeRegistry.Instance.RegisterType("motive.gaming.recipe", typeof(Recipe));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.recipeActivator", typeof(RecipeActivator));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.recipeDeactivator", typeof(RecipeDeactivator));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.recipeActivatedCondition", typeof(RecipeActivatedCondition));

            JsonTypeRegistry.Instance.RegisterType("motive.gaming.inventoryCondition", typeof(InventoryCondition));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.walletCondition", typeof(WalletCondition));

            JsonTypeRegistry.Instance.RegisterType("motive.gaming.canCraftCollectibleCondition", typeof(CanCraftCollectibleCondition));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.canExecuteRecipeCondition", typeof(CanExecuteRecipeCondition));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.canCompleteTaskCondition", typeof(CanCompleteTaskCondition));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.canCompleteObjectiveTaskCondition", typeof(CanCompleteObjectiveTaskCondition));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.quizBase", typeof(QuizBase));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.multipleChoiceQuiz", typeof(MultipleChoiceQuiz));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.freeResponseQuiz", typeof(FreeResponseQuiz));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.quizResponseBase", typeof(QuizResponseBase));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.multipleChoiceResponse", typeof(MultipleChoiceQuizResponse));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.freeResponseAnswerBase", typeof(FreeResponseAnswerBase));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.strictMatchAnswer", typeof(StrictMatchAnswer));
            JsonTypeRegistry.Instance.RegisterType("motive.gaming.textMustContainAnswer", typeof(TextMustContainAnswer));

            JsonTypeRegistry.Instance.RegisterType("motive.ar.beaconTask", typeof(BeaconTask));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.locationTask", typeof(LocationTask));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.visualMarkerTask", typeof(VisualMarkerTask));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.locationMarker", typeof(LocationMarker));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.location3dAsset", typeof(Location3DAsset));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.arCatcherMinigame", typeof(ARCatcherMinigame));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.mapZoomCommand", typeof(MapZoomCommand));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.storyTagLocationTypes", typeof(StoryTagLocationType));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.mapZoomCommand", typeof(MapZoomCommand));

            JsonTypeRegistry.Instance.RegisterType("motive.ar.arLocationCollectionMechanic", typeof(ARLocationCollectionMechanic));
            JsonTypeRegistry.Instance.RegisterType("motive.ar.mapLocationCollectionMechanic", typeof(MapLocationCollectionMechanic));

            JsonTypeRegistry.Instance.RegisterType("motive.attractions.locationAttraction", typeof(LocationAttraction));
            JsonTypeRegistry.Instance.RegisterType("motive.attractions.locationAttractionActivator", typeof(LocationAttractionActivator));
            JsonTypeRegistry.Instance.RegisterType("motive.attractions.locationAttractionInteractible", typeof(LocationAttractionInteractible));
            JsonTypeRegistry.Instance.RegisterType("motive.attractions.locationAttractionContent", typeof(LocationAttractionContent));

            JsonTypeRegistry.Instance.RegisterType("motive.google.polyAsset", typeof(PolyAsset));

            // The Script Resource Processors take the resources from the script processor and
            // direct them to the game components that know what to do with them.
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.core.scriptLauncher", new ScriptLauncherProcessor());

            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ui.objectInspector", new ObjectInspectorProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ui.interfaceUpdate", new InterfaceUpdateProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ui.interfaceDirector", new InterfaceDirectorProcessor());

            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.assignment", new AssignmentProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.objectiveActivator", new ObjectiveActivatorProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.objectiveCompleter", new ObjectiveCompleterProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.playerTask", new PlayerTaskProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.characterTask", new CharacterTaskProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.inventoryCollectibles", new InventoryCollectiblesProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.inventoryCollectibleLimit", new InventoryCollectibleLimitProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.walletCurrency", new WalletCurrencyProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.walletCurrencyLimit", new WalletCurrencyLimitProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.playableContent", new PlayableContentProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.playableContentBatch", new PlayableContentBatchProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.playerReward", new PlayerRewardProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.savePoint", new SavePointProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.recipeActivator", new RecipeActivatorProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.recipeDeactivator", new RecipeDeactivatorProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.gaming.interfaceAction", new InterfaceActionProcessor());

            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.locationTask", new LocationTaskProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.locationMarker", new LocationMarkerProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.location3dAsset", new Location3DAssetProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.ar.locativeAudioContent", new LocativeAudioContentProcessor());

            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.3d.worldObjectEffectPlayer", new WorldObjectEffectPlayerProcessor());

            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.attractions.locationAttractionActivator", new LocationAttractionActivatorProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.attractions.locationAttractionInteractible", new LocationAttractionInteractibleProcessor());
            ScriptEngine.Instance.RegisterScriptResourceProcessor("motive.attractions.locationAttractionContent", new LocationAttractionContentProcessor());

            // Condition monitors track system state and can be used by conditions in the Motive
            // authoring tool.
            ScriptEngine.Instance.RegisterConditionMonitor(new RecipeActivatedConditionMonitor());
            ScriptEngine.Instance.RegisterConditionMonitor(new AppVersionConditionMonitor());
            ScriptEngine.Instance.RegisterConditionMonitor(new TaskObjectiveCompleteConditionMonitor());
            ScriptEngine.Instance.RegisterConditionMonitor(new InventoryConditionMonitor());
            ScriptEngine.Instance.RegisterConditionMonitor(new WalletConditionMonitor());
            ScriptEngine.Instance.RegisterConditionMonitor(new CanCraftCollectibleConditionMonitor());
            ScriptEngine.Instance.RegisterConditionMonitor(new CanExecuteRecipeConditionMonitor());
            ScriptEngine.Instance.RegisterConditionMonitor(new CanCompleteTaskConditionMonitor());
            ScriptEngine.Instance.RegisterConditionMonitor(new CanCompleteObjectiveTaskConditionMonitor());
            ScriptEngine.Instance.RegisterConditionMonitor(WorldObjectProximityConditionMonitor.Instance);
        }
    }
}