using Mutagen.Bethesda;
using System.Collections.Generic;

namespace MasterLimitTest
{
    public partial class Program
    {
        public class PatchContents
        {
            public readonly HashSet<IMajorRecordCommonGetter> records;

            public readonly HashSet<IModContext<IMajorRecordCommonGetter>> contexts;

            public PatchContents()
            {
                records = new();
                contexts = new();
            }

            public PatchContents(RecordsClassifiedByMasters largestMasterRecordSet)
            {
                records = largestMasterRecordSet.recordSet;
                contexts = largestMasterRecordSet.contextSet;
            }
        }

    }
}
