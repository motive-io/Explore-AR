// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Vuforia
{
    public class VuMarkIdentifier
    {
        public string InstanceId { get; set; }
        public string TargetName { get; set; }
        public string DatabaseId { get; set; }

        public override int GetHashCode()
        {
            int hashcode = 0;

            if (InstanceId != null)
            {
                hashcode ^= InstanceId.GetHashCode();
            }

            if (TargetName != null)
            {
                hashcode ^= TargetName.GetHashCode();
            }

            if (DatabaseId != null)
            {
                hashcode ^= DatabaseId.GetHashCode();
            }

            return hashcode;
        }

        public override bool Equals(object obj)
        {
            var other = obj as VuMarkIdentifier;

            if (other != null)
            {
                return 
                    InstanceId == other.InstanceId &&
                    TargetName == other.TargetName &&
                    DatabaseId == other.DatabaseId;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("TargetName={0}, InstanceId={1}, DatabaseId={2}",
                TargetName, InstanceId, DatabaseId);
        }
    }
}
