// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Base class for resources sent to panels along with their activation context.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResourcePanelData<T>
    {
        public ResourceActivationContext ActivationContext { get; set; }
        public T Resource { get; set; }

        public ResourcePanelData(ResourceActivationContext ac, T resource)
        {
            ActivationContext = ac;
            Resource = resource;
        }
    }

}