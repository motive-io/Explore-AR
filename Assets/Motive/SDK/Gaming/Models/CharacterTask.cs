// Copyright (c) 2018 RocketChicken Interactive Inc.

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18052
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Motive.Core.Json;
using Motive.Core.Scripting;
using Motive.Core.Models;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// A task telling the user to take an action with a character.
    /// </summary>
    public class CharacterTask : PlayerTask
    {
        public ObjectReference<Character> CharacterReference { get; set; }

        public override string ImageUrl
        {
            get
            {
                if (base.ImageUrl != null)
                {
                    return base.ImageUrl;
                }

                if (Character != null)
                {
                    return Character.ImageUrl;
                }

                return null;
            }
        }

        public Character Character
        {
            get
            {
                if (CharacterReference != null)
                {
                    return CharacterReference.Object;
                }

                return null;
            }
        }

        public CharacterTask() : this("motive.gaming.characterTask") { }

        public CharacterTask(string objType) : base(objType)
        {
        }
    }
}