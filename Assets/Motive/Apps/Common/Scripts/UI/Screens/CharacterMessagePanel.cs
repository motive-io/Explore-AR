// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using Motive.Core.Media;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using UnityEngine;
using UnityEngine.UI;
using Motive.Unity.Media;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays a Character Message.
    /// </summary>
    /// <seealso cref="Motive.UI.Framework.Panel{ResourcePanelData{Motive.Gaming.Models.CharacterMessage}}" />
    public class CharacterMessagePanel : Panel<ResourcePanelData<CharacterMessage>>
    {
        public override void Populate(ResourcePanelData<CharacterMessage> data)
        {
            PopulateComponent<CharacterMessageComponent>(data.Resource);
        }
    }
}


