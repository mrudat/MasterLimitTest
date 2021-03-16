using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using System;
using System.Threading.Tasks;

namespace MasterLimitTest
{
    public partial class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .AddPatch<IOblivionMod, IOblivionModGetter>(RunPatch)
                .Run(args, new RunPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "YourPatcher.esp",
                        TargetRelease = GameRelease.SkyrimSE,
                    }
                });
        }

        private static void RunPatch<TMod, TModGetter>(IPatcherState<TMod, TModGetter> state)
            where TMod : class, IContextMod<TMod, TModGetter>, TModGetter
            where TModGetter : class, IContextGetterMod<TMod, TModGetter>
        {
            var setFactory = new CustomSetFactory<ModKey>();

            var patchMasterCount = state.PatchMod.MasterReferences.Count;

            if (patchMasterCount <= 1) return;

            // this would usually be 255, but we can't load a mod with 255 masters (yet).
            var MAXIMUM_MASTERS_PER_MOD = patchMasterCount / 2;

            Console.WriteLine($"found {patchMasterCount} master references, attempting to produce a set of mod that each have less than {MAXIMUM_MASTERS_PER_MOD} masters.");

            /// each entry is potentially an emitted mod.
            var recordSets = ClassifyRecordsByReferencedMasters<TMod, TModGetter>(state.PatchMod, setFactory, MAXIMUM_MASTERS_PER_MOD);

            // each entry in here is an emitted mod.
            var patches = PatchesFromRecordSets(recordSets, setFactory, MAXIMUM_MASTERS_PER_MOD);

            // TODO surely there's got to be a better way?
            static TMod NewMod(string name, TModGetter template)
            {
                return template switch
                {
                    SkyrimMod skyrimMod => (TMod)NewSkyrimMod(name, skyrimMod),
                    OblivionMod oblivionMod => (TMod)NewOblivionMod(name, oblivionMod),
                    _ => throw new NotImplementedException(),
                };
            }

            SplitPatchModIntoMultiplePatches<TMod, TModGetter>(state.PatchMod, patches, NewMod);

            Environment.Exit(1);
            throw new NotImplementedException("Profit?");
        }

        private static ISkyrimMod NewSkyrimMod(string modName, ISkyrimModGetter template)
        {
            SkyrimMod newMod = new(ModKey.FromNameAndExtension(modName), template.SkyrimRelease);
            newMod.ModHeader.Flags |= SkyrimModHeader.HeaderFlag.LightMaster;
            return newMod;
        }

        private static IOblivionMod NewOblivionMod(string modName, IOblivionModGetter _) => new OblivionMod(ModKey.FromNameAndExtension(modName));


    }
}
