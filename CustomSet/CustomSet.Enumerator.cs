using System.Collections;
using System.Collections.Generic;

namespace MasterLimitTest
{
    public partial class CustomSet<T> where T : notnull
    {
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly CustomSetFactory<T> factory;
            private readonly IEnumerator<ushort> enumerator;

            public T Current
            {
                get
                {
                    return factory.indexToItem[enumerator.Current];
                }
            }

            object IEnumerator.Current => Current;

            internal Enumerator(CustomSetFactory<T> factory, ushort[] array)
            {
                this.factory = factory;
                this.enumerator = ((IEnumerable<ushort>)array).GetEnumerator();
            }

            public void Dispose() => enumerator.Dispose();

            public bool MoveNext() => enumerator.MoveNext();

            public void Reset() => enumerator.Reset();
        }

    }
}
