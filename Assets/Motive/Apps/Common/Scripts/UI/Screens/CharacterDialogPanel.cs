// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Gaming.Models;
using Motive.UI.Framework;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays a character message that has responses.
    /// </summary>
    /// <seealso cref="CharacterMessagePanel" />
    public class CharacterDialogPanel : CharacterMessagePanel
    {
        public CharacterDialogResponseItem ResponseItem;

        public Table TablePanel;

        private bool m_isListening;

        protected override void Awake()
        {
            base.Awake();

            TablePanel = GetComponentInChildren<Table>();
        }

        protected virtual void OnSelect(CharacterDialogResponseItem item, string option)
        {
            // Only allow one click on this screen.
            if (!m_isListening)
            {
                return;
            }

            m_isListening = false;

            if (option != null)
            {
                Data.ActivationContext.FireEvent(option);
            }

            Back();
        }

        public override void Populate(ResourcePanelData<CharacterMessage> data)
        {
            TablePanel.Clear();

            m_isListening = true;

            foreach (var response in data.Resource.Responses)
            {
                var item = TablePanel.AddItem(ResponseItem);

                item.Populate(response);

                var evtid = response.Event;

                item.OnSelected.AddListener(() => { OnSelect(item, evtid); });
            }

            base.Populate(data);
        }
    }

}