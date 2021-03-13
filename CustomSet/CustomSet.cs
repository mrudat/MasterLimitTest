using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterLimitTest
{
    public partial class CustomSet<T> : IImmutableSet<T>
        where T : notnull
    {
        internal readonly CustomSetFactory<T> factory;

        internal int hashCode = 0;

        internal ushort[] set;

        internal CustomSet(CustomSetFactory<T> factory, ISet<ushort> set)
        {
            this.factory = factory;
            this.set = set.ToArray();
            Array.Sort(this.set);
        }
        internal CustomSet(CustomSet<T>.Builder mutable)
        {
            this.factory = mutable.factory;
            this.set = mutable.set.ToArray();
            Array.Sort(this.set);
            this.hashCode = mutable.hashCode;
        }

        public int Count => set.Length;

        public bool IsReadOnly => true;

        public bool Contains(T value)
        {
            if (!factory.itemToIndex.TryGetValue(value, out _))
                return false;
            if (Array.BinarySearch(set, value) < 0)
                return false;
            return true;
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
            return this;
        }

        public IImmutableSet<T> Add(T item)
        {
            throw new NotImplementedException();
        }

        public IImmutableSet<T> Clear()
        {
            throw new NotImplementedException();
        }

        public CustomSet<T> Except(CustomSet<T> other)
        {
            if (factory != other.factory) throw new NotImplementedException();
            if (other.set.Length == 0) return this;
            if (other.set[0] > set[^1]) return this;
            if (other.set[^1] < set[0]) return this;
            var temp = factory.NewSet();
            temp.set.UnionWith(set);
            temp.set.ExceptWith(other.set);
            return temp.ToCustomSet();
        }

        public IImmutableSet<T> Except(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public IImmutableSet<T> Intersect(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public IImmutableSet<T> Remove(T value)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public IImmutableSet<T> SymmetricExcept(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(T equalValue, out T actualValue)
        {
            throw new NotImplementedException();
        }

        public IImmutableSet<T> Union(IEnumerable<T> other)
        {
            throw new NotImplementedException();
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
            return obj is CustomSet<T> other && this.Equals(other);
        }

        public bool Equals(CustomSet<T> other)
        {
            if (factory != other.factory) return false;
            if (set.Length != other.set.Length) return false;
            foreach (var (First, Second) in set.Zip(other.set))
                if (First != Second)
                    return false;
            return true;
        }
    }
}
