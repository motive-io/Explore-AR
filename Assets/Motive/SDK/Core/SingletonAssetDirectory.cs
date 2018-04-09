// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Core
{
    public class SingletonAssetDirectory<T, A> : AssetDirectory<A> 
        where A : ScriptObject
        where T : new()
    {
        static T g_instance;

        public static T Instance
        {
            get
            {
                if (g_instance == null)
                {
                    g_instance = new T();
                }

                return g_instance;
            }
        }
    }
}
