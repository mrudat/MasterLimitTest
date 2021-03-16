using Mutagen.Bethesda;
using System.Collections.Generic;

namespace MasterLimitTest
{
    public partial class Program
    {

        public static List<HashSet<IModContext<TMod, TModGetter, IMajorRecordCommon, IMajorRecordCommonGetter>>> PatchesFromRecordSets<TMod, TModGetter>(Dictionary<CustomSet<ModKey>, RecordsClassifiedByMasters<TMod, TModGetter>> recordSets, CustomSetFactory<ModKey> setFactory, int maximumMastersPerMod = MAXIMUM_MASTERS_PER_MOD)
            where TMod : class, IMod, TModGetter
            where TModGetter : class, IModGetter, IMajorRecordContextEnumerable<TMod, TModGetter>
        {
            var patches = new List<HashSet<IModContext<TMod, TModGetter, IMajorRecordCommon, IMajorRecordCommonGetter>>>();

            bool newRecordsFirst = true;

            var temp = setFactory.NewSet();
            var smallestSets = new Dictionary<CustomSet<ModKey>, NewStruct2>();

            while (recordSets.Count > 0)
            {
                temp.Clear();
                int largestMasterSetCount = 0;
                CustomSet<ModKey> largestMasterSet = null!;

                foreach (var (masterSet, data) in recordSets)
                {
                    if (newRecordsFirst)
                    {
                        if (!data.hasNewRecords) continue;
                    }
                    else
                    {
                        temp.UnionWith(masterSet);
                    }

                    if (largestMasterSet is null || data.MasterCount > largestMasterSetCount)
                    {
                        largestMasterSetCount = data.MasterCount;
                        largestMasterSet = masterSet;
                    }
                }

                if (largestMasterSet is null)
                {
                    newRecordsFirst = false;
                    continue;
                }

                if (!newRecordsFirst)
                {
                    if (temp.Count <= maximumMastersPerMod)
                    {
                        var patchContents = new HashSet<IModContext<TMod, TModGetter, IMajorRecordCommon, IMajorRecordCommonGetter>>();
                        foreach (var (masterSet, data) in recordSets)
                        {
                            patchContents.UnionWith(data.contextSet);
                        }
                        recordSets.Clear();
                        patches.Add(patchContents);
                        continue;
                    }
                }

                var largestMasterRecordSet = recordSets[largestMasterSet];
                recordSets.Remove(largestMasterSet);

                while (recordSets.Count > 0)
                {
                    int smallestAdditionalMasterSetCount = 0;
                    CustomSet<ModKey> smallestMasterSet = null!;
                    smallestSets.Clear();
                    foreach (var (masterSet, data) in recordSets)
                    {
                        if (newRecordsFirst)
                            if (!data.hasNewRecords)
                                continue;

                        // the set of distinct masterRecords that would be added to our current target if we added this recordSet to the mix.

                        var temp2 = masterSet.Except(largestMasterSet);

                        int additionalMasterCount = temp2.Count;

                        if (additionalMasterCount == smallestAdditionalMasterSetCount)
                        {
                            var foo = smallestSets.Autovivify(temp2, () => new(0, new()));
                            foo.recordCount += data.contextSet.Count;
                            foo.masterSets.Add(masterSet);
                        }

                        if (smallestMasterSet is null || additionalMasterCount < smallestAdditionalMasterSetCount)
                        {
                            smallestSets.Clear();
                            smallestSets[temp.ToCustomSet()] = new(data.contextSet.Count, new() { masterSet });
                            smallestAdditionalMasterSetCount = additionalMasterCount;
                            smallestMasterSet = masterSet;
                        }
                    }

                    if (smallestMasterSet is null)
                    {
                        newRecordsFirst = false;
                        continue;
                    }

                    if (smallestAdditionalMasterSetCount == 0)
                    {
                        foreach (var (_, (_, masterSets)) in smallestSets)
                        {
                            foreach (var masterSet in masterSets)
                            {
                                var recordSet = recordSets[masterSet];
                                recordSets.Remove(masterSet);

                                largestMasterRecordSet.contextSet.UnionWith(recordSet.contextSet);
                            }
                        }
                    }
                    else
                    {
                        int newMasterCount = largestMasterSetCount + smallestAdditionalMasterSetCount;

                        if (newMasterCount > maximumMastersPerMod)
                            break;

                        largestMasterSetCount = newMasterCount;

                        int maxCount = 0;
                        HashSet<CustomSet<ModKey>> victim = null!;

                        foreach (var (id, (count, masterSets)) in smallestSets)
                        {
                            if (victim is null || count > maxCount)
                            {
                                maxCount = count;
                                victim = masterSets;
                            }
                        }

                        foreach (var masterSet in victim)
                        {
                            var recordSet = recordSets[masterSet];
                            recordSets.Remove(masterSet);

                            largestMasterRecordSet.contextSet.UnionWith(recordSet.contextSet);
                        }
                    }
                }
                newRecordsFirst = false;
                patches.Add(largestMasterRecordSet.contextSet);
            }


            return patches;
        }

    }
}
