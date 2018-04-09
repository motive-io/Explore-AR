// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Defines one way to activate an active entity.
    /// </summary>
    public class EntityActionActivationOption
    {
        /// <summary>
        /// Player must spend this to activate the entity.
        /// </summary>
        public ValuablesCollection Cost { get; set; }
        /// <summary>
        /// Player must hold these items to activate an entity.
        /// For example, can specify a certain XP that the player needs.
        /// </summary>
        public ValuablesCollection Requirements { get; set; }
        /// <summary>
        /// Optional attributes that must match when two entities interact.
        /// </summary>
        public EntityActionAttributeValue[] Attributes { get; set; }
    }
}
