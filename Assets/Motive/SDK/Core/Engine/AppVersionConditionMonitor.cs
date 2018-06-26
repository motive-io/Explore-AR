// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using Motive.Core.Models;

namespace Motive.Core.Scripting
{
    public class AppVersionConditionMonitor : SynchronousConditionMonitor<AppVersionCondition>
    {
        Version m_appVersion;

        public AppVersionConditionMonitor()
            : base("motive.core.appVersionCondition")
        {
            m_appVersion = Parse(UnityEngine.Application.version);
        }

        Version Parse(string versionString)
        {
            var parts = versionString.Split('.');

            if (parts.Length > 0)
            {
                int major = 0;
                int minor = 0;
                int build = 0;
                int revision = 0;

                if (parts.Length >= 1)
                {
                    major = int.Parse(parts[0]);
                }

                if (parts.Length >= 2)
                {
                    minor = int.Parse(parts[1]);
                }

                if (parts.Length >= 3)
                {
                    build = int.Parse(parts[2]);
                }

                if (parts.Length >= 4)
                {
                    revision = int.Parse(parts[3]);
                }

                return new Version(major, minor, build, revision);
            }

            return null;
        }

        public override bool CheckState(FrameOperationContext fop, AppVersionCondition condition, out object[] results)
        {
            results = null;

            if (condition.Version == null)
            {
                return false;
            }

            var version = Parse(condition.Version);

            return Motive.Core.Utilities.MathHelper.CheckNumericalCondition(
                condition.Operator.GetValueOrDefault(NumericalConditionOperator.GreaterThanOrEqual),
                m_appVersion, version);

            //return m_appVersion >= version;
        }
    }
}