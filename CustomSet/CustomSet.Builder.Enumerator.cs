using System.Collections;
using System.Collections.Generic;

namespace MasterLimitTest
{
    public partial class CustomSet<T> where T : notnull
    {
        public partial class Builder
        {
            public struct Enumerator : IEnumerator<T>, IEnumerator
            {
                private readonly CustomSetFactory<T> factory;
                private readonly IEnumerator<ushort> enumerator;

                public T Current => factory.indexToItem[enumerator.Current];

                object IEnumerator.Current => Current;

                internal Enumerator(CustomSetFactory<T> factory, ISet<ushort> set)
                {
                    this.factory = factory;
                    this.enumerator = set.GetEnumerator();
                }

                public void Dispose() => enumerator.Dispose();

                public bool MoveNext() => enumerator.MoveNext();

                public void Reset() => enumerator.Reset();
            }
        }
    }
}
