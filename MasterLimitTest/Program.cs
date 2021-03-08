using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Collections;
using Noggog;

namespace MasterLimitTest
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .Run(args, new RunPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "YourPatcher.esp",
                        TargetRelease = GameRelease.SkyrimSE,
                    }
                });
        }

        public static int CountBits(BitArray array)
        {
            // TODO https://stackoverflow.com/questions/5063178/counting-bits-set-in-a-net-bitarray-class
            int count = 0;
            for (int i = 0; i < array.Length; i++)
                if (array.Get(i))
                    count++;
            return count;
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var patchMasterCount = state.PatchMod.MasterReferences.Count;
            var patchModKey = state.PatchMod.ModKey;

            // this would usually be 255, but we can't load a mod with 255 masters (yet).
            var targetMasterCount = patchMasterCount / 2;

            var masterModKeyToIndex = state.PatchMod.MasterReferences
                .Select((x, i) => new KeyValuePair<ModKey,int>(x.Master, i))
                .ToImmutableDictionary();

            var linkCache = state.PatchMod.ToUntypedImmutableLinkCache();

            /// each entry is potentially an emitted mod.
            var recordSets = new Dictionary<BitArray, (int masterCount, bool hasNewRecords, HashSet<FormKey> recordSet)>();

            /// these absolutely have to remain in patchMod, but can probably be an empty stub.
            var newRecords = new HashSet<FormKey>();
            var newRecordsMasters = new BitArray(patchMasterCount);

            {
                BitArray masterSet = new(patchMasterCount);
                int recordMasterCount = 0;

                foreach (var record in state.PatchMod.EnumerateMajorRecords())
                {
                    masterSet.SetAll(false);
                    recordMasterCount = 0;

                    var formKey = record.FormKey;

                    void RegisterModKey(ModKey modKey)
                    {
                        if (modKey == patchModKey) return;
                        int index = masterModKeyToIndex[modKey];
                        if (masterSet.Get(index)) return;
                        masterSet.Set(index, true);
                        recordMasterCount++;
                    }

                    RegisterModKey(formKey.ModKey);

                    foreach (var link in record.ContainedFormLinks)
                    {
                        RegisterModKey(link.FormKey.ModKey);
                    }

                    if (recordMasterCount > targetMasterCount)
                        throw RecordException.Factory(new Exception($"Too many masters {recordMasterCount} referenced by one record"), record);

                    if (!recordSets.TryGetValue(masterSet, out var recordSet))
                        recordSet = recordSets[masterSet] = (recordMasterCount, false, new());
                    recordSet.recordSet.Add(formKey);

                    if (formKey.ModKey == patchModKey)
                    {
                        recordSet.hasNewRecords = true;
                        newRecords.Add(formKey);
                        newRecordsMasters.And(masterSet);
                    }
                }
            }

            var newRecordsMastersCount = CountBits(newRecordsMasters);

            var patches = new List<HashSet<FormKey>>();

            bool newRecordsFirst = true;

            // might not be required?
            if (newRecordsMastersCount > targetMasterCount)
            {
                // we can't include all new records in full in patchMod, so attempt to pack as many as possible into patchMod
                PackRecordsIntoPatch();
            }

            while(recordSets.Count > 0)
            {
                PackRecordsIntoPatch();
            }

            void PackRecordsIntoPatch()
            {
                var recordSetsInPatch = new Dictionary<BitArray, (int masterCount, bool hasNewRecords, HashSet<FormKey> recordSet)>();

                BitArray temp = new(patchMasterCount);
                int largestMasterSetCount = 0;
                BitArray largestMasterSet = null!;

                foreach (var (masterSet, data) in recordSets)
                {
                    if (newRecordsFirst)
                        if (!data.hasNewRecords)
                            continue;
                    temp.Or(masterSet);
                    if (data.masterCount > largestMasterSetCount)
                    {
                        largestMasterSetCount = data.masterCount;
                        largestMasterSet = masterSet;
                    }
                }

                var masterCount = CountBits(temp);

                if (masterCount <= targetMasterCount)
                {
                    var temp2 = recordSets[largestMasterSet].recordSet;
                    recordSets.Remove(largestMasterSet);
                    foreach (var (masterSet, data) in recordSets)
                    {
                        temp2.UnionWith(data.recordSet);
                    }
                    recordSets.Clear();
                    patches.Add(temp2);
                }
                else
                {
                    var largestMasterSetData = recordSets[largestMasterSet];
                    recordSets.Remove(largestMasterSet);

                    int smallestAdditionalMasterSetCount = 0;
                    BitArray smallestMasterSet = null!;

                    while (true)
                    {
                        smallestAdditionalMasterSetCount = 0;
                        smallestMasterSet = null!;
                        foreach (var (masterSet, data) in recordSets)
                        {
                            if (newRecordsFirst)
                                if (!data.hasNewRecords)
                                    continue;

                            temp.SetAll(false);
                            temp.Or(masterSet);
                            temp.Not();
                            temp.Or(largestMasterSet);
                            temp.Not();

                            int additionalMasterCount = CountBits(temp);

                            if (smallestMasterSet is null || additionalMasterCount < smallestAdditionalMasterSetCount)
                            {
                                smallestAdditionalMasterSetCount = additionalMasterCount;
                                smallestMasterSet = masterSet;
                            }
                        }

                        if (smallestMasterSet is null)
                        {
                            newRecordsFirst = false;
                            continue;
                        }

                        int newMasterCount = largestMasterSetCount + smallestAdditionalMasterSetCount;

                        if (newMasterCount > targetMasterCount)
                        {
                            patches.Add(largestMasterSetData.recordSet);
                            return;
                        }

                        largestMasterSetCount = newMasterCount;

                        var smallestMasterSetData = recordSets[smallestMasterSet];
                        recordSets.Remove(smallestMasterSet);

                        largestMasterSetData.recordSet.UnionWith(smallestMasterSetData.recordSet);

                        if (recordSets.Count == 0)
                        {
                            patches.Add(largestMasterSetData.recordSet);
                            return;
                        }
                    }
                }
            }

            var firstPatch = patches[0];
            patches.RemoveAt(0);

            foreach (var formKeySet in patches)
            {
                // TODO create new mod.
                var mod = state.PatchMod;

                foreach (var formKey in formKeySet)
                {
                    var modKey = formKey.ModKey;
                    if (modKey == patchModKey)
                    {
                        // TODO add form to mod as override, clear out all formLinks in parent form.
                    }
                    else
                    {
                        // add form to mod, remove from patchMod.
                    }
                }
            }
        }

    }
}
