using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterLimitTest
{
    public partial class Program
    {
        private static void SplitPatchModIntoMultiplePatches(IPatcherState<ISkyrimMod, ISkyrimModGetter> state, List<HashSet<FormKey>> patches)
        {
            var patchMod = state.PatchMod;
            var patchModKey = patchMod.ModKey;
            var linkCache = state.LinkCache;

            // break here to investigate the results.
            var firstPatch = patches[0];
            patches.RemoveAt(0);

            int modCount = 0;

            foreach (var formKeySet in patches)
            {
                var newModKey = ModKey.FromNameAndExtension($"Synthesis_{modCount}.esp");

                var newMod = new SkyrimMod(newModKey, patchMod.SkyrimRelease);
                newMod.ModHeader.Flags |= SkyrimModHeader.HeaderFlag.LightMaster;

                int newCount = 0;
                int overrideCount = 0;

                foreach (var formKey in formKeySet)
                {
                    var modKey = formKey.ModKey;
                    var form = linkCache.Resolve(formKey);

                    // TODO add form to mod as override
                    // newMod.Add(form);

                    if (modKey == patchModKey)
                    {
                        // this is an entirely new form

                        // TODO clear out all formLinks patchMod

                        newCount++;
                    }
                    else
                    {
                        // this overrides an existing form.

                        patchMod.Remove(formKey);

                        overrideCount++;
                    }
                }

                Console.WriteLine($"{newMod} has {newCount} 'new' records and {overrideCount} overrides");

                modCount++;
            }
        }

    }
}
