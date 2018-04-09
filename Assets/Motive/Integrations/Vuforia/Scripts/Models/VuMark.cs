// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Vuforia
{
    public class VuMark : ScriptObject, IVuforiaMarker, IMediaItemProvider
    {
        public string InstanceId { get; set; }
        public VuMarkTarget Target { get; set; }
        public bool UseExtendedTracking { get; set; }

        public void GetMediaItems(IList<Core.Media.MediaItem> items)
        {
            if (Target != null)
            {
                Target.GetMediaItems(items);
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

                    m_identifier.InstanceId = InstanceId;

                    if (Target != null)
                    {
                        m_identifier.TargetName = Target.Name;

                        if (Target.Database != null)
                        {
                            m_identifier.DatabaseId = Target.Database.Id;
                        }
                    }
                }

                return m_identifier;
            }
        }

        public MarkerDatabase Database
        {
            get
            {
                if (Target != null)
                {
                    return Target.Database;
                }

                return null;
            }
        }
    }
}
