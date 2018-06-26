// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class RecipeDeactivatorProcessor : ScriptResourceProcessor<RecipeDeactivator>
    {
        public override void ActivateResource(ResourceActivationContext context, RecipeDeactivator resource)
        {
            if (!context.IsClosed)
            {
                if (resource.RecipeReferences != null)
                {
                    foreach (var r in resource.RecipeReferences)
                    {
                        ActivatedRecipeManager.Instance.DeactivateRecipe(r.ObjectId);
                    }
                }

                context.Close();
            }
        }
    }
}