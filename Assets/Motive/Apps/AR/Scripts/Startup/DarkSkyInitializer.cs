using System.Collections;
using System.Collections.Generic;
using Motive.AR.WeatherServices;
using Motive.Unity.Gaming;
using UnityEngine;

namespace Motive.Unity.Apps
{
    public class DarkSkyInitializer : Initializer
    {
        public string ApiKey;

        ForecastIOWeatherService m_weatherService;

        protected override void Initialize()
        {
            if (ApiKey != null)
            {
                m_weatherService = new ForecastIOWeatherService(ApiKey);

                WeatherMonitor.Instance.Initialize(m_weatherService);
            }
        }
    }
}
