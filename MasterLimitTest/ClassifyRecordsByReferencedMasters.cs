using Mutagen.Bethesda;
using System;
using System.Collections.Generic;

namespace MasterLimitTest
{
    public partial class Program
    {
        /// <summary>
        /// The maximum number of masters supported by the mod format.
        /// 
        /// not readonly so that we don't need stupidly large test cases.
        /// </summary>
        public const int MAXIMUM_MASTERS_PER_MOD = 255;

        public static Dictionary<CustomSet<ModKey>, RecordsClassifiedByMasters> ClassifyRecordsByReferencedMasters(
            IModGetter patchMod,
            CustomSetFactory<ModKey> setFactory,
            int maximumMastersPerMod = MAXIMUM_MASTERS_PER_MOD)
        {
            var recordSets = new Dictionary<CustomSet<ModKey>, RecordsClassifiedByMasters>();
            var masterSetBuilder = setFactory.NewSet();

            var patchModKey = patchMod.ModKey;

            foreach (var context in patchMod.EnumerateOnlyMajorRecordsWithContexts())
            {
                masterSetBuilder.Clear();

                bool isNewRecord = false;

                IModContext? thisContext = context;
                while(thisContext is not null)
                {
                    masterSetBuilder.Add(context.ModKey);

                    var record = context.Record;
                    var modKey = record.FormKey.ModKey;
                    masterSetBuilder.Add(modKey);

                    if (modKey == patchModKey)
                        isNewRecord = true;

                    // TODO Does this include all child records?
                    foreach (var link in record.ContainedFormLinks)
                        masterSetBuilder.Add(link.FormKey.ModKey);

                    thisContext = thisContext.Parent;

                    int recordMasterCount = masterSetBuilder.Count;
                    if (recordMasterCount > maximumMastersPerMod)
                        throw RecordException.Factory(new Exception($"Too many masters ({recordMasterCount}) referenced by one record"), record);
                }

                CustomSet<ModKey> masterSet = masterSetBuilder.ToCustomSet();
                var recordSet = recordSets.Autovivify(masterSet, () => new(masterSet));

                recordSet.contextSet.Add(context);

                if (isNewRecord)
                    recordSet.hasNewRecords = true;
            }

            foreach (var record in patchMod.EnumerateContextFreeMajorRecords())
            {
                masterSetBuilder.Clear();
                int recordMasterCount = 0;
                var formKey = record.FormKey;

                masterSetBuilder.Add(formKey.ModKey);

                foreach (var link in record.ContainedFormLinks)
                    masterSetBuilder.Add(link.FormKey.ModKey);

                recordMasterCount = masterSetBuilder.Count;

                if (recordMasterCount > maximumMastersPerMod)
                    throw RecordException.Factory(new Exception($"Too many masters ({recordMasterCount}) referenced by one record"), record);

                CustomSet<ModKey> masterSet = masterSetBuilder.ToCustomSet();
                var recordSet = recordSets.Autovivify(masterSet, () => new(masterSet));

                recordSet.recordSet.Add(record);

                if (formKey.ModKey == patchModKey)
                    recordSet.hasNewRecords = true;
            }

            return recordSets;
        }
    }
}
