using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MasterLimitTest
{
    public partial class Program
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

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var setFactory = new CustomSetFactory<ModKey>();

            var patchMasterCount = state.PatchMod.MasterReferences.Count;

            if (patchMasterCount <= 1) return;

            // this would usually be 255, but we can't load a mod with 255 masters (yet).
            var MAXIMUM_MASTERS_PER_MOD = patchMasterCount / 2;

            Console.WriteLine($"found {patchMasterCount} master references, attempting to produce a set of mod that each have less than {MAXIMUM_MASTERS_PER_MOD} masters.");

            /// each entry is potentially an emitted mod.
            var recordSets = ClassifyRecordsByReferencedMasters(state.PatchMod, setFactory, MAXIMUM_MASTERS_PER_MOD);

            // each entry in here is an emitted mod.
            var patches = PatchesFromRecordSets(recordSets, setFactory, MAXIMUM_MASTERS_PER_MOD);

            SplitPatchModIntoMultiplePatches(state, patches);

            Environment.Exit(1);
            throw new NotImplementedException("Profit?");
        }

    }
}
