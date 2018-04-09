// Copyright (c) 2018 RocketChicken Interactive Inc.
#if MOTIVE_IAP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Models;
using Motive.UI.Framework;
using UnityEngine;

class PurchasableEpisodeSelectPanel : ScriptRunnerSelectPanel
{

	// Purchase Manager must be initialized before this called
	public void PopulateTableItems(){
		Debug.Log ("pop table items");

		Table.Clear();


	    var showDev = (ShowInDevelopment && BuildSettings.IsDebug);
	    var showDraft = (ShowDraft && BuildSettings.IsDebug);

		var orderMap = new Dictionary<EpisodeSelectState, int> () {
			{ EpisodeSelectState.Running, 0 },
			{ EpisodeSelectState.AvailableToLaunch, 1 },
			{ EpisodeSelectState.Disabled, 1 },
			{ EpisodeSelectState.Finished, 2 }
		};

		var episodes = EpisodeManager.Instance.GetEpisodes (showDev, showDraft);
			
		if (EpisodePurchaseManager.Instance.IsInitialized ()) {
			episodes = episodes.OrderBy (item => orderMap [EpisodePurchaseManager.Instance.GetEpisodeState (item)])
								.ThenBy (item => item.Order);
		} else {
			episodes = episodes.OrderBy (item => item.Order);
		}


		foreach (var item in episodes) {
			var itemObj = Table.AddItem (SelectItem);
			itemObj.Populate (item);

			itemObj.Launch += (sender, args) => {
				EpisodeManager.Instance.LaunchEpisode (args.Episode);

				Close ();
			};

			itemObj.Stop += (sender, args) => {
				EpisodeManager.Instance.StopEpisode (args.Episode);

				itemObj.UpdateState ();
			};

			itemObj.Reset += (sender, args) => {
				EpisodeManager.Instance.ResetEpisode (args.Episode);

				itemObj.UpdateState ();
			};

			var _item = item; //closure
			itemObj.OnSelected.AddListener (() => {
				PanelManager.Instance.Show (PanelManager.Instance.GetPanel<PurchasableEpisodeDetailsPanel> (), _item, Populate);
			});
		}
	}

	public override void Populate ()
	{
		Table.Clear ();

		if (!EpisodePurchaseManager.Instance.IsInitialized())  {
			Debug.Log ("calling purchasemanager initialize from episode select panel");
			EpisodePurchaseManager.Instance.InitializeComplete += PopulateTableItemsCallback;
			EpisodePurchaseManager.Instance.InitializeFailed += PopulateTableItemsCallback;
			EpisodePurchaseManager.Instance.Initialize();
		} else {
			Debug.Log ("skipped calling purchasemanager initialize from episode select panel");
			PopulateTableItems();
		}

	}

	public override void DidPop ()
	{
		base.DidPop ();
		EpisodePurchaseManager.Instance.InitializeComplete -= PopulateTableItemsCallback;
		EpisodePurchaseManager.Instance.InitializeFailed -= PopulateTableItemsCallback;

	}

	void PopulateTableItemsCallback(object sender, EventArgs e){
		PopulateTableItems();
	}
}
#endif