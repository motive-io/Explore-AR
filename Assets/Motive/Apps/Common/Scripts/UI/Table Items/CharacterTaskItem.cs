// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.Unity.Apps;
using Motive.Unity.Utilities;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class CharacterTaskItem : TaskItem
    {
        public override void PopulateImage(RawImage image, string url)
        {
            if (url != null)
            {
                base.PopulateImage(image, url);
            }
            else
            {
                var charTask = Driver.Task as CharacterTask;

                if (charTask != null)
                {
                    if (charTask.ImageUrl == null)
                    {
                        // Override with character image if available
                        var character = CharacterDirectory.Instance.GetItem(charTask.CharacterReference);

                        if (character != null && Image)
                        {
                            Image.gameObject.SetActive(true);
                            ImageLoader.LoadImageOnMainThread(character.ImageUrl, Image);
                        }
                    }
                }
            }
        }
    }

}