using Mutagen.Bethesda;
using System;
using System.Collections.Generic;

namespace MasterLimitTest
{
    public partial class Program
    {
        public class RecordsClassifiedByMasters
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
            public readonly HashSet<IMajorRecordCommonGetter> recordSet;

            public int MasterCount { get => masterSet.Count; }

            public RecordsClassifiedByMasters(CustomSet<ModKey> masterSet, bool hasNewRecords, HashSet<IMajorRecordCommonGetter> recordSet)
            {
                this.masterSet = masterSet;
                this.hasNewRecords = hasNewRecords;
                this.recordSet = recordSet;
            }

            public override bool Equals(object? obj)
            {
                return obj is RecordsClassifiedByMasters other &&
                       MasterCount == other.MasterCount &&
                       hasNewRecords == other.hasNewRecords &&
                       EqualityComparer<HashSet<IMajorRecordCommonGetter>>.Default.Equals(recordSet, other.recordSet);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(MasterCount, hasNewRecords, recordSet);
            }

            public void Deconstruct(out CustomSet<ModKey> masterSet, out bool hasNewRecords, out HashSet<IMajorRecordCommonGetter> recordSet)
            {
                masterSet = this.masterSet;
                hasNewRecords = this.hasNewRecords;
                recordSet = this.recordSet;
            }

            public static implicit operator (CustomSet<ModKey> masterSet, bool hasNewRecords, HashSet<IMajorRecordCommonGetter> recordSet)(RecordsClassifiedByMasters value)
            {
                return (value.masterSet, value.hasNewRecords, value.recordSet);
            }

            public static implicit operator RecordsClassifiedByMasters((CustomSet<ModKey> masterSet, bool hasNewRecords, HashSet<IMajorRecordCommonGetter> recordSet) value)
            {
                return new RecordsClassifiedByMasters(value.masterSet, value.hasNewRecords, value.recordSet);
            }
        }
    }
}