// Copyright (c) 2018 RocketChicken Interactive Inc.
#if MOTIVE_IAP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Models;
using Motive.UI.Framework;
using Motive.Unity.Apps;
using Motive.Unity.Utilities;
using UnityEngine;
using UnityEngine.UI;


class PurchasableEpisodeDetailsPanel : Panel<ScriptDirectoryItem>
{
    public Text Title;
    public Text Description;

    /// <summary>
    /// These objects are shown/hidden based on the episode's state
    /// 
    /// If one list of objects is shown, all others are hidden. 
    /// Exception is Disabled and NotPurchased can be shown together
    /// </summary>
    private List<GameObject[]> DisplayLists;
    public GameObject[] DisplayWhenNotPurchased;
    public GameObject[] DisplayWhenDisabled;
    public GameObject[] DisplayWhenAvailToLaunch;
    public GameObject[] DisplayWhenRunning;
    public GameObject[] DisplayWhenFin;
    public GameObject[] DisplayDebug;

    protected EpisodeSelectState EpisodeSelectState;

    protected override void Awake()
    {
        DisplayLists = new List<GameObject[]>
        {
            DisplayWhenNotPurchased,
            DisplayWhenDisabled,
            DisplayWhenAvailToLaunch,
            DisplayWhenRunning,
            DisplayWhenFin
        };

        base.Awake();
    }

    public override void Populate(ScriptDirectoryItem item)
    {
        Title.text = item.Title;
        if (Description != null && !string.IsNullOrEmpty(item.Description))
        {
            Description.text = item.Description;
        }

        DetermineState();
        UpdateViewState();
    }

    private void ItemPurchased(object sender, ItemPurchasedEventArgs itemPurchasedEventArgs)
    {
        if (itemPurchasedEventArgs.ProductIdentifier == Data.ProductIdentifier)
        {
            DetermineState();
            UpdateViewState();
        }
    }
    public override void DidPush()
    {
        EpisodePurchaseManager.Instance.ItemPurchased += ItemPurchased;
    }



    public override void DidPop()
    {
        EpisodePurchaseManager.Instance.ItemPurchased -= ItemPurchased;
    }

    public void SetState(EpisodeSelectState selectState)
    {
        this.EpisodeSelectState = selectState;
    }
    public virtual void DetermineState()
    {
        SetState(EpisodePurchaseManager.Instance.GetEpisodeState(this.Data));
    }

    public virtual void UpdateViewState()
    {
		DisplayLists.ForEach(l => ObjectHelper.SetObjectsActive (l, false) );

        var purchasable = !string.IsNullOrEmpty(Data.ProductIdentifier);

		// show everything and return
		//if (BuildSettings.IsDebug) {
		//	DisplayLists.ForEach(l => ObjectHelper.SetObjectsActive (l, true) );
		//	return;
		//}

        switch (EpisodeSelectState)
        {
            case EpisodeSelectState.Disabled:
				ObjectHelper.SetObjectsActive (DisplayWhenDisabled, true);

			    if (purchasable && EpisodePurchaseManager.Instance.IsInitialized)
                {
                    var wasPurchased = false;
                    EpisodePurchaseManager.Instance.CheckProductReceipt(Data.ProductIdentifier, () =>
                    {
                        wasPurchased = true;
                    });
                    if (!wasPurchased)
                    {
						ObjectHelper.SetObjectsActive (DisplayWhenNotPurchased, true);
                    }
                }
                break;
			case EpisodeSelectState.AvailableToLaunch:
				ObjectHelper.SetObjectsActive (DisplayWhenAvailToLaunch, true);
                break;
            case EpisodeSelectState.Running:
				ObjectHelper.SetObjectsActive (DisplayWhenRunning, true);
                break;
            case EpisodeSelectState.Finished:
				ObjectHelper.SetObjectsActive (DisplayWhenFin, true);
                break;
            default:
                break;
        }

        if (BuildSettings.IsDebug && DisplayDebug != null && DisplayDebug.Any())
        {
            ObjectHelper.SetObjectsActive(DisplayDebug, true);
        }
    }

    public void DoLaunch()
    {
        if (Data != null)
        {
            ScriptRunnerManager.Instance.Launch(Data);

            DetermineState();
            UpdateViewState();
        }
    }

    public void DoStop()
    {
        if (Data != null)
        {
            ScriptRunnerManager.Instance.Stop(Data);

            DetermineState();
            UpdateViewState();
        }
    }

    public void DoReset()
    {
        if (Data != null)
        {
            ScriptRunnerManager.Instance.Reset(Data);

            DetermineState();
            UpdateViewState();

        }
    }

    public void DoPurchase()
    {
		if (Data != null && EpisodePurchaseManager.Instance.IsInitialized)
        {
            EpisodePurchaseManager.Instance.BuyProductID(Data.ProductIdentifier);
        }
    }
}
#endif