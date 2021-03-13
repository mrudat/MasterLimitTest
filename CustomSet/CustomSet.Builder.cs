using Mutagen.Bethesda;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MasterLimitTest
{
    public partial class CustomSet<T> where T : notnull
    {
        public partial class Builder : ISet<T>
        {
            internal readonly CustomSetFactory<T> factory;

            internal ImmutableHashSet<ushort>.Builder set;

            internal int hashCode = 0;

            internal Builder(CustomSetFactory<T> factory)
            {
                this.factory = factory;

                set = ImmutableHashSet.CreateBuilder<ushort>();
            }

            public int Count => set.Count;

            public bool IsReadOnly => false;

            public bool Add(T item) => set.Add(factory.ItemToIndex(item));

            public void Clear() => set.Clear();

            public bool Contains(T item)
            {
                if (!factory.itemToIndex.TryGetValue(item, out var index))
                    return false;
                return set.Contains(index);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new Enumerator(factory, set);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(factory, set);
            }

            public IImmutableSet<T> ToImmutable()
            {
                return this.ToCustomSet();
            }

            public CustomSet<T> ToCustomSet()
            {
                return new CustomSet<T>(this);
            }

            void ICollection<T>.Add(T item)
            {
                set.Add(factory.ItemToIndex(item));
                hashCode = 0;
            }

            public void ExceptWith(CustomSet<T> other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                hashCode = 0;
                set.ExceptWith(other.set);
            }

            public void ExceptWith(CustomSet<T>.Builder other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                hashCode = 0;
                set.ExceptWith(other.set);
            }

            public void ExceptWith(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public void IntersectWith(CustomSet<T> other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                hashCode = 0;
                set.IntersectWith(other.set);
            }

            public void IntersectWith(CustomSet<T>.Builder other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                hashCode = 0;
                set.IntersectWith(other.set);
            }

            public void IntersectWith(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool IsProperSubsetOf(CustomSet<T> other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                return set.IsProperSubsetOf(other.set);
            }

            public bool IsProperSubsetOf(CustomSet<T>.Builder other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                return set.IsProperSubsetOf(other.set);
            }

            public bool IsProperSubsetOf(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool IsProperSupersetOf(CustomSet<T> other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                return set.IsProperSupersetOf(other.set);
            }

            public bool IsProperSupersetOf(CustomSet<T>.Builder other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                return set.IsProperSupersetOf(other.set);
            }

            public bool IsProperSupersetOf(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool IsSubsetOf(CustomSet<T> other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                return set.IsSubsetOf(other.set);
            }

            public bool IsSubsetOf(CustomSet<T>.Builder other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                return set.IsSubsetOf(other.set);
            }

            public bool IsSubsetOf(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool IsSupersetOf(CustomSet<T> other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                return set.IsSupersetOf(other.set);
            }

            public bool IsSupersetOf(CustomSet<T>.Builder other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                return set.IsSupersetOf(other.set);
            }

            public bool IsSupersetOf(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool Overlaps(CustomSet<T> other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                return set.Overlaps(other.set);
            }

            public bool Overlaps(CustomSet<T>.Builder other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                return set.Overlaps(other.set);
            }

            public bool Overlaps(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool Remove(T item)
            {
                if (!factory.itemToIndex.TryGetValue(item, out var index))
                    return false;
                hashCode = 0;
                return set.Remove(index);
            }

            public bool SetEquals(CustomSet<T> other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                return set.SetEquals(other.set);
            }

            public bool SetEquals(CustomSet<T>.Builder other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                return set.SetEquals(other.set);
            }

            public bool SetEquals(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public void SymmetricExceptWith(CustomSet<T> other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                hashCode = 0;
                set.SymmetricExceptWith(other.set);
            }

            public void SymmetricExceptWith(CustomSet<T>.Builder other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                hashCode = 0;
                set.SymmetricExceptWith(other.set);
            }

            public void SymmetricExceptWith(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public void UnionWith(CustomSet<T> other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                hashCode = 0;
                set.UnionWith(other.set);
            }

            public void UnionWith(CustomSet<T>.Builder other)
            {
                if (factory != other.factory) throw new NotImplementedException();
                hashCode = 0;
                set.UnionWith(other.set);
            }

            public void UnionWith(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                using var enumerator = set.GetEnumerator();
                for (int i = arrayIndex; i < array.Length; i++)
                {
                    array[i] = factory.indexToItem[enumerator.Current];
                    if (!enumerator.MoveNext())
                        return;
                }
            }

            public override int GetHashCode()
            {
                if (hashCode != 0)
                    return hashCode;
                var temp = new HashCode();
                temp.Add(factory.GetHashCode());
                foreach (var item in set)
                    temp.Add(item);
                hashCode = temp.ToHashCode();
                if (hashCode == 0)
                    hashCode = 1;
                return hashCode;
            }

            public override bool Equals(object? obj)
            {
                return obj is CustomSet<T>.Builder other && this.Equals(other);
            }

            public bool Equals(CustomSet<T>.Builder obj)
            {
                if (factory != obj.factory) return false;
                return set.Equals(obj.set);
            }
        }
    }
}
