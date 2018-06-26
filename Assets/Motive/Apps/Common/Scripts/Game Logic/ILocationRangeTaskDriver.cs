// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Interface for task drivers that support the notion of a range.
    /// </summary>
    public interface ILocationRangeTaskDriver : IPlayerTaskDriver
    {
        DoubleRange ActionRange { get; }
    }
}