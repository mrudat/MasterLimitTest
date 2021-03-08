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

            // ModKey -> index into BitArray for the flag indicating that this ModKey is present.
            var masterModKeyToIndex = state.PatchMod.MasterReferences
                .Select((x, i) => new KeyValuePair<ModKey,int>(x.Master, i))
                .ToImmutableDictionary();

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

            // each entry in here is an emitted mod.
            var patches = new List<HashSet<FormKey>>();

            bool newRecordsFirst = true;

            // might not be required?
            if (newRecordsMastersCount > targetMasterCount)
            {
                // we can't include all new records in full in patchMod, so attempt to pack as many as possible into patchMod
                newRecordsFirst = true;
            }

            BitArray temp = new(patchMasterCount);
            BitArray largestMasterSet = null!;
            BitArray smallestMasterSet = null!;
            var smallestSets = new HashSet<BitArray>();

            while (recordSets.Count > 0)
            {
                temp.SetAll(false);
                int largestMasterSetCount = 0;
                largestMasterSet = null!;

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

                // at this point temp has a bit set for every distinct master in recordSets.
                if (CountBits(temp) <= targetMasterCount)
                {
                    var temp2 = recordSets[largestMasterSet].recordSet;
                    recordSets.Remove(largestMasterSet);
                    foreach (var (masterSet, (_, _, recordSet)) in recordSets)
                        temp2.UnionWith(recordSet);
                    recordSets.Clear();
                    patches.Add(temp2);
                    newRecordsFirst = false;
                    continue;
                }

                var largestMasterRecordSet = recordSets[largestMasterSet].recordSet;
                recordSets.Remove(largestMasterSet);

                int smallestAdditionalMasterSetCount = 0;

                while (recordSets.Count > 0)
                {
                    smallestAdditionalMasterSetCount = 0;
                    smallestMasterSet = null!;
                    smallestSets.Clear();
                    foreach (var (masterSet, data) in recordSets)
                    {
                        if (newRecordsFirst)
                            if (!data.hasNewRecords)
                                continue;

                        // the set of distinct masterRecords that would be added to our current target if we added this recordSet to the mix.

                        // temp = masterSet.Subtract(largestMasterSet);
                        // temp = masterSet & !largestMasterSet
                        // temp = !(!masterSet | largestMasterSet)
                        temp.SetAll(false);
                        temp.Or(masterSet);
                        temp.Not();
                        temp.Or(largestMasterSet);
                        temp.Not();

                        int additionalMasterCount = CountBits(temp);

                        if (additionalMasterCount == smallestAdditionalMasterSetCount)
                        {
                            smallestSets.Add(masterSet);
                        }

                        if (smallestMasterSet is null || additionalMasterCount < smallestAdditionalMasterSetCount)
                        {
                            smallestSets.Clear();
                            smallestSets.Add(masterSet);
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
                        foreach (var masterSet in smallestSets)
                        {
                            var recordSet = recordSets[masterSet].recordSet;
                            recordSets.Remove(masterSet);

                            largestMasterRecordSet.UnionWith(recordSet);
                        }
                    }
                    else
                    {
                        // TODO find the best candidate from smallestSets

                        int newMasterCount = largestMasterSetCount + smallestAdditionalMasterSetCount;

                        if (newMasterCount > targetMasterCount)
                            break;

                        largestMasterSetCount = newMasterCount;

                        var recordSet = recordSets[smallestMasterSet].recordSet;
                        recordSets.Remove(smallestMasterSet);

                        largestMasterRecordSet.UnionWith(recordSet);
                    }
                }
                newRecordsFirst = false;
                patches.Add(largestMasterRecordSet);
            }

            // break here to investigate the results.
            var firstPatch = patches[0];
            patches.RemoveAt(0);

            int modCount = 0;

            foreach (var formKeySet in patches)
            {
                // TODO create new mod.
                var mod = state.PatchMod;

                int newCount = 0;
                int overrideCount = 0;

                foreach (var formKey in formKeySet)
                {
                    var modKey = formKey.ModKey;
                    // TODO add form to mod as override

                    if (modKey == patchModKey)
                    {
                        // this is an entirely new masterRecord

                        // TODO clear out all formLinks patchMod
                        newCount++;
                    }
                    else
                    {
                        // this overrides an existing masterRecord.

                        // TODO remove from patchMod.
                        overrideCount++;
                    }
                }

                Console.WriteLine($"patch_{modCount}.esl has {newCount} new records and {overrideCount} overrides");

                modCount++;
            }
        }

    }
}
