// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Attaches to game objects to expose MapController commands.
    /// </summary>
    public class MapControllerCommands : MonoBehaviour
    {
        public void CenterMap()
        {
            MapController.Instance.CenterMap();
        }
    }
}