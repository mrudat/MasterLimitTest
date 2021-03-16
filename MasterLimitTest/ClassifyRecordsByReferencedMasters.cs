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

        public static Dictionary<CustomSet<ModKey>, RecordsClassifiedByMasters<TMod, TModGetter>> ClassifyRecordsByReferencedMasters<TMod, TModGetter>(
            TModGetter patchMod,
            CustomSetFactory<ModKey> setFactory,
            int maximumMastersPerMod = MAXIMUM_MASTERS_PER_MOD)
            where TMod : class, IMod, TModGetter
            where TModGetter : class, IModGetter, IMajorRecordContextEnumerable<TMod, TModGetter>
        {
            var recordSets = new Dictionary<CustomSet<ModKey>, RecordsClassifiedByMasters<TMod,TModGetter>>();
            var masterSetBuilder = setFactory.NewSet();

            var patchModKey = patchMod.ModKey;

            var linkCache = patchMod.ToUntypedImmutableLinkCache();

            foreach (IModContext<TMod, TModGetter, IMajorRecordCommon, IMajorRecordCommonGetter>? context in patchMod.EnumerateMajorRecordContexts<IMajorRecordCommon, IMajorRecordCommonGetter>(linkCache, false))
            {
                masterSetBuilder.Clear();

                bool isNewRecord = false;

                IModContext? thisContext = context;
                while(thisContext is not null)
                {
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

            return recordSets;
        }
    }
}
