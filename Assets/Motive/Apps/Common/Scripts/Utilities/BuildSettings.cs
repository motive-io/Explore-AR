// Copyright (c) 2018 RocketChicken Interactive Inc.

namespace Motive.Unity.Utilities
{
    public enum BuildConfig
    {
        Debug,
        Release
    }

    /// <summary>
    /// Manage the build configuration for the project.
    /// </summary>
    public class BuildSettings : SingletonComponent<BuildSettings>
    {
        public BuildConfig BuildConfig;

        public static bool IsDebug
        {
            get
            {
                return BuildSettings.Instance ?
                    BuildSettings.Instance.BuildConfig == BuildConfig.Debug :
                    true;
            }
        }

        public static bool IsRelease
        {
            get
            {
                return BuildSettings.Instance ?
                    BuildSettings.Instance.BuildConfig == BuildConfig.Release :
                    false;
            }
        }
    }

}