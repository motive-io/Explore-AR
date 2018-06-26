// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Attractions.Models;
using Motive.Core.Scripting;
using Motive.UI.Framework;
using Motive.Unity.Apps.Attractions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    public class LocationAttractionItem : TextImageItem
    {
        public GameObject ARIndicator;
        public GameObject AudioIndicator;
        public GameObject VideoIndicator;

        public Table InteractiveItemTable;

        protected override void Awake()
        {
            base.Awake();

            if (!InteractiveItemTable)
            {
                InteractiveItemTable = GetComponentInChildren<Table>();
            }
        }

        void AddInteractibleItemIndicator(IScriptObject item)
        {
            if (InteractiveItemTable)
            {
                if (item is ILocationAugmentedOptions && ARIndicator)
                {
                    InteractiveItemTable.AddItem(ARIndicator);
                }
            }
        }

        public void Populate(ActivatedAttractionContext ctxt)
        {
            var attraction = ctxt.Attraction;

            if (InteractiveItemTable)
            {
                InteractiveItemTable.Clear();
            }

            SetText(attraction.Title);
            SetImage(attraction.ImageUrl);

            /*
            var interactibles = ctxt.Interactibles;

            if (interactibles != null)
            {
                foreach (var ia in interactibles)
                {
                    /*
                    if (ia.InteractibleItem != null)
                    {
                        AddInteractibleItemIndicator(ia.InteractibleItem);
                    }* /
                }
            }

            var content = ctxt.Content;

            if (content != null)
            {
                foreach (var ia in content)
                {
                    /*
                    if (ia.Content != null)
                    {
                        //AddContentIndicator(ia.Content);
                    }* /
                }
            }*/
        }
    }
}