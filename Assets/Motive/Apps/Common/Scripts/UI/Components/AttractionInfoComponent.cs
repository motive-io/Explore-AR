// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Attractions.Models;
using Motive.UI.Framework;
using Motive.Unity.Apps.Attractions;
using Motive.Unity.Maps;
using Motive.Unity.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class AttractionInfoComponent : PanelComponent<ActivatedAttractionContext>
    {
        public Text Title;
        public Text Subtitle;
        public Text Description;
        public RawImage Image;

        public Table Table;
        public TextImageItem ARTodoItem;

        protected override void Awake()
        {
            base.Awake();

            if (!Table)
            {
                Table = GetComponentInChildren<Table>();
            }
        }

        public override void DidShow(ActivatedAttractionContext obj)
        {
            Table.Clear();

            if (Title)
            {
                Title.text = obj.Attraction.Title;
            }

            if (Description)
            {
                Description.text = obj.Attraction.Description;
            }

            ImageLoader.LoadImageOnThread(obj.Attraction.ImageUrl, Image);

            var todos = obj.ToDoItems;

            foreach (var handler in obj.ToDoItems)
            {
                var item = handler.ItemProperties.GetItem();

                if (item is LocationAugmentedImage)
                {
                    var todo = Table.AddItem(ARTodoItem);
                    todo.Text.text = handler.ItemProperties.Title;

                    todo.OnSelected.AddListener(() =>
                        {

                        });
                }
            }

            base.DidShow(obj);
        }
    }
}