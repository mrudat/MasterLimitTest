using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Synthesis;
using System;

namespace MasterLimitTest
{
    public partial class Program
    {
        private static void RunPatch(IPatcherState<IOblivionMod, IOblivionModGetter> state)
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

            SplitPatchModIntoMultiplePatches(state.PatchMod, patches, NewMod, AddContextToMod);

            Environment.Exit(1);
            throw new NotImplementedException("Profit?");
        }
        private static OblivionMod NewMod(string modName, IOblivionMod template) => new(ModKey.FromNameAndExtension(modName));

        public static void AddContextToMod(IOblivionMod arg1, IModContext<IMajorRecordCommonGetter> arg2)
        {
            throw new NotImplementedException();
        }

    }
}
