// Copyright (c) 2018 RocketChicken Interactive Inc.


using JsonFx.Json;
using Motive.Core.Models;
using Motive.Core.Scripting;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Represents a number of collectibles.
    /// </summary>
    public class CollectibleCount : ScriptObject
    {
        public ObjectReference<Collectible> CollectibleReference;
        public int Count;

        [JsonIgnore]
        public Collectible Collectible
        {
            get
            {
                return CollectibleReference != null ? CollectibleReference.Object : null;
            }
        }

        [JsonIgnore]
        public string CollectibleId
        {
            get
            {
                return CollectibleReference != null ? CollectibleReference.ObjectId : null;
            }
        }

        public CollectibleCount()
        {
            // Default for Count is 1 unless othewise specified
            Count = 1;
        }

        public CollectibleCount(string collectibleId, int count)
        {
            CollectibleReference = new ObjectReference<Collectible> { ObjectId = collectibleId };
            Count = count;
        }

        public override string ToString()
        {
            return string.Format("[CollectibleCount: CollectibleId={0}, Count={1}]", CollectibleReference == null ? "null" : CollectibleReference.ObjectId, Count);
        }
    }
}