using Mutagen.Bethesda;
using System;
using System.Collections.Generic;

namespace MasterLimitTest
{
    public partial class Program
    {
        public class RecordsClassifiedByMasters<TMod,TModGetter>
            where TMod : class, IMod, TModGetter
            where TModGetter : class, IModGetter
        {
            /// <summary>
            /// The set of masters that these records share.
            /// </summary>
            public readonly CustomSet<ModKey> masterSet;

            /// <summary>
            /// has at least one record that is not an override of an existing record.
            /// </summary>
            public bool hasNewRecords = false;

            /// <summary>
            /// the set of contexts that share this combination of master records
            /// </summary>
            public readonly HashSet<IModContext<TMod, TModGetter, IMajorRecordCommon, IMajorRecordCommonGetter>> contextSet = new();

            public int MasterCount { get => masterSet.Count; }

            public RecordsClassifiedByMasters(CustomSet<ModKey> masterSet)
            {
                this.masterSet = masterSet;
            }
        }
    }
}