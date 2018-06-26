// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Vuforia
{
    //motive.vuforia.imageMarker
    public class ImageTarget : ScriptObject, IVuforiaMarker, IMediaItemProvider
    {
        public string Name { get; set; }
        public MarkerDatabase Database { get; set; }
        public bool UseExtendedTracking { get; set; }

        public void GetMediaItems(IList<Core.Media.MediaItem> items)
        {
            if (Database != null)
            {
                Database.GetMediaItems(items);
            }
        }

        private VuMarkIdentifier m_identifier;

        public VuMarkIdentifier Identifier
        {
            get
            {
                if (m_identifier == null)
                {
                    m_identifier = new VuMarkIdentifier();

                    m_identifier.TargetName = Name;

                    if (Database != null)
                    {
                        m_identifier.DatabaseId = Database.Id;
                    }
                }

                return m_identifier;
            }
        }
    }
}
