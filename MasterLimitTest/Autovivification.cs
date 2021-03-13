using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterLimitTest
{
    public static class Autovivification
    {
        public static V Autovivify<K, V>(this IDictionary<K, V> dict, K key) where K : notnull where V : new() => dict.Autovivify(key, () => new());

        public static V Autovivify<K, V>(this IDictionary<K, V> dict, K key, Func<V> newThing) where K : notnull
        {
            if (!dict.TryGetValue(key, out var value))
                value = dict[key] = newThing();
            return value;
        }
    }
}
