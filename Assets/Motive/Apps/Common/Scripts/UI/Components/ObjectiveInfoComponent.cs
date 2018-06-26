// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays objective information.
    /// </summary>
    public class ObjectiveInfoComponent : PanelComponent<TaskObjective>
    {
        public Text Title;
        public GameObject TitleLayout;
        public Text Description;
        public GameObject DescriptionLayout;
        public RawImage Image;
        public GameObject ImageLayout;

        public override void Populate(TaskObjective obj)
        {
            SetText(TitleLayout, Title, obj.Title);
            SetText(DescriptionLayout, Description, obj.Description);
            SetImage(ImageLayout, Image, obj.ImageUrl);

            base.Populate(obj);
        }
    }

}