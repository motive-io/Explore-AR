using Motive.AR.Media;
using System;

namespace Motive.Unity.Apps
{
    public class LocativeAudioInitializer : Initializer
    {
        public bool StartOnInit = true;

        /// <summary>
        /// How long to fade in the audio when a user moves in range from the
        /// outer edge of the disc.
        /// </summary>
        public float OuterFadeInDuration = 5f;
        /// <summary>
        /// How long to fade out the audio when a user moves out of range from the
        /// outer edge of the disc.
        /// </summary>
        public float OuterFadeOutDuration = 10f;
        /// <summary>
        /// How long to fade in the audio when a user moves in range from the
        /// inner edge of the disc if a min range is specified.
        /// </summary>
        public float InnerFadeInDuration = 5f;
        /// <summary>
        /// How long to fade out the audio when a user moves out of range from the
        /// inner edge of the disc if a min range is specified.
        /// </summary>
        public float InnerFadeOutDuration = 10f;

        protected override void Initialize()
        {
            LocativeAudioDriver.Instance.Engine.DefaultOuterFadeInDuration = TimeSpan.FromSeconds(OuterFadeInDuration);
            LocativeAudioDriver.Instance.Engine.DefaultOuterFadeOutDuration = TimeSpan.FromSeconds(OuterFadeOutDuration);

            LocativeAudioDriver.Instance.Engine.DefaultInnerFadeInDuration = TimeSpan.FromSeconds(InnerFadeInDuration);
            LocativeAudioDriver.Instance.Engine.DefaultInnerFadeOutDuration = TimeSpan.FromSeconds(InnerFadeOutDuration);

            if (StartOnInit)
            {
                LocativeAudioDriver.Instance.Start();
            }
        }
    }
}
