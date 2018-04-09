// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using System.Collections;
using System.Collections.Generic;
using Motive.Gaming.Models;
using UnityEngine;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class RecipeActivatorProcessor : ScriptResourceProcessor<RecipeActivator>
    {
        public override void ActivateResource(ResourceActivationContext context, RecipeActivator resource)
        {
            if (resource.Recipes != null)
            {
                foreach (var r in resource.Recipes)
                {
                    ActivatedRecipeManager.Instance.ActivateRecipe(r);
                }
            }
        }

        public override void DeactivateResource(ResourceActivationContext context, RecipeActivator resource)
        {
            // do nothing right???
        }
    }
}