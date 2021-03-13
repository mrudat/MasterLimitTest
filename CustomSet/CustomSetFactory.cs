using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MasterLimitTest
{
    public class CustomSetFactory<T>
        where T : notnull
    {
        internal readonly Dictionary<T, ushort> itemToIndex = new();

        internal readonly List<T> indexToItem = new();

        private readonly int hashCode;

        public override int GetHashCode() => hashCode;

        public CustomSetFactory()
        {
            hashCode = RuntimeHelpers.GetHashCode(this);
        }

        public CustomSet<T>.Builder NewSet()
        {
            return new CustomSet<T>.Builder(this);
        }

        internal ushort ItemToIndex(T item)
        {
            if (itemToIndex.TryGetValue(item, out var index))
                return index;
            index = checked((ushort)indexToItem.Count);
            indexToItem.Add(item);
            itemToIndex[item] = index;

            return index;
        }

        internal T? IndexToItem(int index)
        {
            if (index > itemToIndex.Count)
                return default;
            return indexToItem[index];
        }

    }
}
