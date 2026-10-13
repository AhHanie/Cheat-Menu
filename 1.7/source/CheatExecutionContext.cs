using System.Collections.Generic;
using Verse;

namespace Cheat_Menu
{
    public sealed class CheatExecutionContext
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public CheatExecutionContext(CheatDefinition cheat)
        {
            Cheat = cheat;
        }

        public CheatDefinition Cheat { get; }

        public LocalTargetInfo LastTarget { get; set; }

        public bool TryGet<T>(string key, out T value)
        {
            object rawValue;
            if (values.TryGetValue(key, out rawValue) && rawValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default(T);
            return false;
        }

        public T Get<T>(string key, T fallback = default(T))
        {
            T value;
            return TryGet(key, out value) ? value : fallback;
        }

        public void Set<T>(string key, T value)
        {
            values[key] = value;
        }
    }
}
