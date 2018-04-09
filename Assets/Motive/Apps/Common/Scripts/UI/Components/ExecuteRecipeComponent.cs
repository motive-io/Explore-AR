// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Manages the "execute recipe" flow.
    /// </summary>
    public class ExecuteRecipeComponent : PanelComponent<Recipe>
    {
        public Panel ExecuteRecipePanel;

        void Start()
        {
            if (!ExecuteRecipePanel)
            {
                ExecuteRecipePanel = PanelManager.Instance.GetPanel<ExecuteRecipePanel>();
            }
        }

        public override void DidShow()
        {
            if (ExecuteRecipePanel)
            {
                PanelManager.Instance.Pop(ExecuteRecipePanel);
            }

            base.DidShow();
        }

        public override void Populate(Recipe recipe)
        {
            if (ExecuteRecipePanel)
            {
                PanelManager.Instance.Push(ExecuteRecipePanel, recipe);
            }
        }
    }
}