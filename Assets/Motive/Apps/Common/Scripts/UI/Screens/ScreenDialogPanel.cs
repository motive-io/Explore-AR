// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections.Generic;
using Motive.Gaming.Models;
using Motive.UI.Framework;

namespace Motive.Unity.UI
{
    public class ScreenDialogPanel : ScreenMessagePanel
    {
        protected bool m_isListening;

        public ScreenDialogResponseItem ResponseItem;
        public Table TablePanel;

        protected List<ScreenDialogResponseItem> Items { get; private set; }


        protected override void Awake()
        {
            base.Awake();

            TablePanel = GetComponentInChildren<Table>();

            Items = new List<ScreenDialogResponseItem>();
        }


        protected virtual void OnSelect(ScreenDialogResponseItem item, string option)
        {
            // Only allow one click on this screen.
            if (!m_isListening)
            {
                return;
            }

            m_isListening = false;

            Back();

            if (option != null)
            {
                Data.ActivationContext.FireEvent(option);
            }
        }


        public override void Populate(ResourcePanelData<ScreenMessage> data)
        {
            base.Populate(data);

            Items.Clear();

            TablePanel.Clear();

            m_isListening = true;


            if (data.Resource.MediaItem != null)
            {
            }

            foreach (var response in data.Resource.Responses)
            {
                var item = TablePanel.AddItem(ResponseItem);

                item.Populate(response);

                var eventId = response.Event;

                item.OnSelected.AddListener(() => { OnSelect(item, eventId); });

                Items.Add(item);
            }


            base.Populate(data);
        }
    }
}