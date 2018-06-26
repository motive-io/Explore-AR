// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Panel that handles executing a recipe. This panel is designed
    /// for recipes with up to 3 inputs and one collectible output.
    /// </summary>
    public class ExecuteRecipePanel : TablePanel<Recipe>
    {
        public Text RecipeTitle;
        public Text ItemTitle;
        public Text ItemDescription;

        public Text OutputText;
        public Button ExecuteButton;
        public RawImage OutputImage;
        public InventoryTableItem IngredientItem;
        public Color NeedsMoreTextColor = Color.red;
        public GameObject[] ShowWhenCantExecute;
        public GameObject[] ShowWhenCanExecute;
        public Text CantExecuteText;
        public Button LaunchCrafterButton;
        public PanelLink CraftScreen;

        public bool CloseOnExecute;

        public int NumCrafted { get; private set; }

        private Collectible m_output;
        private int _maxCrafts;

        public int NumberToCraft = 1;
        public Text CraftCount;

        public UnityEvent OnExecuteRecipe;

        void SetCraftCount()
        {
            if (CraftCount)
            {
                if (NumberToCraft >= _maxCrafts)
                {
                    CraftCount.text = "MAX (" + _maxCrafts + ")";
                }
                else
                {
                    CraftCount.text = NumberToCraft.ToString();
                }
            }
        }

        public void IncreaseCraftAmount()
        {
            if (NumberToCraft < _maxCrafts)
            {
                NumberToCraft++;
            }
            SetCraftCount();
            PopulateIngredients();
        }

        public void DecreaseCraftAmount()
        {
            if (NumberToCraft > 0)
            {
                NumberToCraft--;
            }

            SetCraftCount();

            PopulateIngredients();
        }

        void PopulateIngredients()
        {
            Table.Clear();

            var recipe = Data;

            if (recipe != null)
            {
                if (recipe.Output != null &&
                    recipe.Output.CollectibleCounts != null &&
                    recipe.Output.CollectibleCounts.Length > 0)
                {
                    // This panel is designed to look at one collectible as output.
                    m_output = recipe.Output.CollectibleCounts[0].Collectible;

                    if (m_output != null)
                    {
                        ImageLoader.LoadImageOnMainThread(m_output.ImageUrl, OutputImage);

                        if (ItemTitle)
                        {
                            ItemTitle.text = m_output.Title;
                        }

                        if (ItemDescription)
                        {
                            ItemDescription.text = m_output.Description;
                        }
                    }
                }

                if (RecipeTitle != null)
                {
                    RecipeTitle.text = recipe.Title;
                }

                if (recipe.Ingredients != null)
                {
                    if (recipe.Ingredients.CollectibleCounts != null)
                    {
                        foreach (var cc in recipe.Ingredients.CollectibleCounts)
                        {
                            var collectible = cc.Collectible;
                            var count = Inventory.Instance.GetCount(cc.CollectibleId);

                            if (Table)
                            {
                                var item = Table.AddItem(IngredientItem);

                                item.Populate(collectible);
                                if (item.NumberText)
                                {
                                    item.NumberText.gameObject.SetActive(true);
                                    item.NumberText.text = StringFormatter.GetGiveCountString(count, cc.Count * NumberToCraft, NeedsMoreTextColor);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                CantExecute("NO RECIPES!");
            }

            CheckState();
        }

        public override void Populate(Recipe recipe)
        {
            NumCrafted = 0;

            _maxCrafts = CraftingHelper.GetMaxNumberOfCrafts(recipe);
            NumberToCraft = 1; // Math.Min(_maxCrafts, 1);

            //if (t)
            //{
            //    t.text = _maxCrafts.ToString();
            //}

            m_output = null;

            if (OutputImage)
            {
                OutputImage.texture = null;
            }

            SetCraftCount();

            PopulateIngredients();

            base.Populate(recipe);
        }

        public void PushCraftScreen()
        {
            var data = Data;

            if (CraftScreen)
            {
                var p = CraftScreen.GetPanel<ExecuteRecipePanel>();
                CraftScreen.Push(Data, () =>
                    {
                        if (p && p.NumCrafted > 0)
                        {
                            if (OnExecuteRecipe != null)
                            {
                                OnExecuteRecipe.Invoke();
                            }

                            if (CloseOnExecute)
                            {
                                Back();
                            }
                        }
                    });
            }
        }

        void CantExecute(string message)
        {
            if (ExecuteButton)
            {
                ExecuteButton.interactable = false;
            }

            if (LaunchCrafterButton)
            {
                LaunchCrafterButton.interactable = false;
            }

            ObjectHelper.SetObjectsActive(ShowWhenCantExecute, true);
            ObjectHelper.SetObjectsActive(ShowWhenCanExecute, false);

            if (CantExecuteText)
            {
                CantExecuteText.text = message;
            }
        }

        void CheckState()
        {
            ObjectHelper.SetObjectsActive(ShowWhenCantExecute, false);
            ObjectHelper.SetObjectsActive(ShowWhenCanExecute, true);

            if (m_output != null)
            {
                if (OutputText)
                {
                    OutputText.text = m_output.Title;
                }
            }
            else
            {
                if (OutputText)
                {
                    OutputText.text = null;
                }
            }

            var reason = CraftingHelper.GetRecipeReason(Data);

            if (OutputText)
            {
                OutputText.gameObject.SetActive(true);
            }

            if (CantExecuteText)
            {
                CantExecuteText.gameObject.SetActive(true);
            }

            switch (reason)
            {
                case CraftingReason.RequirementsMet:
                    if (ExecuteButton)
                    {
                        ExecuteButton.interactable = true;
                    }
                    if (LaunchCrafterButton)
                    {
                        LaunchCrafterButton.interactable = true;
                        LaunchCrafterButton.onClick.AddListener(PushCraftScreen);

                        if (OutputText)
                        {
                            OutputText.gameObject.SetActive(false);
                        }
                    }

                    if (CantExecuteText)
                    {
                        CantExecuteText.gameObject.SetActive(false);
                    }
                    break;
                case CraftingReason.RequirementsUnmet:
                    CantExecute("Requirements have not been met.");
                    break;
                case CraftingReason.XpUnmet:
                    CantExecute(reason.ToString());
                    break;
                case CraftingReason.NoRequirements:
                case CraftingReason.NoRecipe:
                default:
                    break;
            }
        }

        public void ExecuteRecipe()
        {
            for (var i = 0; i < NumberToCraft; i++)
            {
                if (Data != null)
                {
                    // Make sure in case someone gets clever.
                    if (!TransactionManager.Instance.HasValuables(Data.Ingredients))
                    {
                        return;
                    }

                    NumCrafted++;

                    Motive.Platform.Instance.FireInteractionEvent("craftItem");

                    TransactionManager.Instance.Exchange(Data.Ingredients, Data.Output);
                }
            }

            if (NumCrafted > 0)
            {
                if (CloseOnExecute)
                {
                    Back();
                }

                if (OnExecuteRecipe != null)
                {
                    OnExecuteRecipe.Invoke();
                }
            }
            else
            {
                Populate(Data);
            }
        }

        public override void DidHide()
        {
            if (OutputImage)
            {
                OutputImage.texture = null;
            }

            base.DidHide();
        }

        public void Show(Collectible collectible, Action onClose)
        {
            var recipes = RecipeDirectory.Instance.GetRecipesForCollectible(collectible.Id);

            // For now only look at one recipe per collectible
            var recipe = (recipes == null) ? null : recipes.FirstOrDefault();

            PanelManager.Instance.Show(this, recipe, onClose);
        }
    }

}