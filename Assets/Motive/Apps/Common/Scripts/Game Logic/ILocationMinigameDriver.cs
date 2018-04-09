// Copyright (c) 2018 RocketChicken Interactive Inc.

namespace Motive.Unity.Gaming
{
    public interface ILocationMinigameDriver : IMinigameDriver
    {
        string ActionButtonText { get; }

        string OutOfRangeActionButtonText { get; }

        bool ShowMapAnnotation { get; }

        bool ShowActionButton { get; }
    
    } 
}
