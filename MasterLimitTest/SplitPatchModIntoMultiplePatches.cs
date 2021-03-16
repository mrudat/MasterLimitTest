using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MasterLimitTest
{
    public partial class Program
    {
        public static List<TMod> SplitPatchModIntoMultiplePatches<TMod,TModGetter>(
            TMod patchMod,
            List<HashSet<IModContext<TMod, TModGetter, IMajorRecordCommon, IMajorRecordCommonGetter>>> patches,
            Func<string, TModGetter, TMod> NewMod)
            where TMod : class, IMod, TModGetter
            where TModGetter : class, IModGetter
        {
            List<TMod> mods = new();
            var patchModKey = patchMod.ModKey;

            // break here to investigate the results.
            var firstPatch = patches[0];
            patches.RemoveAt(0);

            int modCount = 0;

            mods.Add(patchMod);

            foreach (var patchContents in patches)
            {
                TMod newMod = NewMod($"Synthesis_{modCount}.esp", patchMod);

                mods.Add(newMod);

                int newCount = 0;
                int overrideCount = 0;

                foreach (var context in patchContents)
                {
                    var modKey = context.ModKey;

                    context.GetOrAddAsOverride(newMod);
                    patchMod.Remove(context.Record);

                    if (modKey == patchModKey)
                    {
                        // this is an entirely new form

                        // .. ?

                        newCount++;
                    }
                    else
                    {
                        // this overrides an existing form.

                        // .. ?

                        overrideCount++;
                    }
                }

                Console.WriteLine($"{newMod} has {newCount} 'new' records and {overrideCount} overrides");

                modCount++;
            }

            return mods;
        }

    }

}
