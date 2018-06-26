// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class TaskDetailsPanel : Panel<IPlayerTaskDriver>
    {
        public RawImage Image;
        public Text Title;

        public override void Populate(IPlayerTaskDriver data)
        {
            if (Image && data.Task.ImageUrl != null)
            {
                ImageLoader.LoadImageOnThread(data.Task.ImageUrl, Image);
            }

            if (Title)
            {
                Title.text = data.Task.Title;
            }

            base.Populate(data);
        }
    }

}