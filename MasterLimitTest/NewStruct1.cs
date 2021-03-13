using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;

namespace MasterLimitTest
{
    public partial class Program
    {
        public class NewStruct1
        {
            /// <summary>
            /// The set of masters that these records share.
            /// </summary>
            public readonly CustomSet<ModKey> masterSet;

            /// <summary>
            /// has at least one record that is not an override of an existing record.
            /// </summary>
            public bool hasNewRecords;

            /// <summary>
            /// the set of records that share this combination of master records
            /// </summary>
            public readonly HashSet<FormKey> recordSet;

            public int MasterCount { get => masterSet.Count; }

            public NewStruct1(CustomSet<ModKey> masterSet, bool hasNewRecords, HashSet<FormKey> recordSet)
            {
                this.masterSet = masterSet;
                this.hasNewRecords = hasNewRecords;
                this.recordSet = recordSet;
            }

            public override bool Equals(object? obj)
            {
                return obj is NewStruct1 other &&
                       MasterCount == other.MasterCount &&
                       hasNewRecords == other.hasNewRecords &&
                       EqualityComparer<HashSet<FormKey>>.Default.Equals(recordSet, other.recordSet);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(MasterCount, hasNewRecords, recordSet);
            }

            public void Deconstruct(out CustomSet<ModKey> masterSet, out bool hasNewRecords, out HashSet<FormKey> recordSet)
            {
                masterSet = this.masterSet;
                hasNewRecords = this.hasNewRecords;
                recordSet = this.recordSet;
            }

            public static implicit operator (CustomSet<ModKey> masterSet, bool hasNewRecords, HashSet<FormKey> recordSet)(NewStruct1 value)
            {
                return (value.masterSet, value.hasNewRecords, value.recordSet);
            }

            public static implicit operator NewStruct1((CustomSet<ModKey> masterSet, bool hasNewRecords, HashSet<FormKey> recordSet) value)
            {
                return new NewStruct1(value.masterSet, value.hasNewRecords, value.recordSet);
            }
        }
    }
}