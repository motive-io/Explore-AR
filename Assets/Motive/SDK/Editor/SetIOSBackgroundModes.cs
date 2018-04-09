using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Callbacks;
using System.Collections;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;
using Motive;

public class SetIOSBackgroundModes : MonoBehaviour {
    #if UNITY_IOS
	[PostProcessBuild]
	public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject) {

		if (buildTarget == BuildTarget.iOS) {
			var scenes = EditorBuildSettings.scenes;

			Platform platform = null;

			foreach (var sceneInfo in scenes)
			{
				var scene = EditorSceneManager.OpenScene(sceneInfo.path);

				foreach (var go in scene.GetRootGameObjects())
				{
					platform = go.GetComponentInChildren<Platform>();

					if (platform != null)
					{
						break;
					}
				}
			}
			/*
			// Get plist
			*/

			if (platform != null)
			{
				var backgroundAudio = platform.EnableBackgroundAudio;
				var backgroundLocation = platform.EnableBackgroundLocation;

				if (backgroundAudio || backgroundLocation)
				{
					string plistPath = pathToBuiltProject + "/Info.plist";
					PlistDocument plist = new PlistDocument();
					plist.ReadFromString(File.ReadAllText(plistPath));

					// Get root
					PlistElementDict rootDict = plist.root;

					var backgroundModes = rootDict.CreateArray("UIBackgroundModes");

					if (backgroundAudio)
					{
						backgroundModes.AddString("audio");
					}

					if (backgroundLocation)
					{
						backgroundModes.AddString("location");
						rootDict.SetString("NSLocationAlwaysUsageDescription", "This app uses your location in the background.");
					}

					rootDict.SetString("NSLocationUsageDescription", "This app uses your location.");

					File.WriteAllText(plistPath, plist.WriteToString());
				}
			}
		}
	}
#endif
}
