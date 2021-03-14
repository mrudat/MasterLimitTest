using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;

namespace MasterLimitTest
{
    public partial class Program
    {
        public static List<T> SplitPatchModIntoMultiplePatches<T>(
            T patchMod,
            List<HashSet<IMajorRecordCommonGetter>> patches,
            Func<string, T, T> NewMod)
            where T : IMod
        {
            List<T> mods = new();
            var patchModKey = patchMod.ModKey;

            // break here to investigate the results.
            var firstPatch = patches[0];
            patches.RemoveAt(0);

            int modCount = 0;

            mods.Add(patchMod);

            foreach (var formKeySet in patches)
            {
                T newMod = NewMod($"Synthesis_{modCount}.esp", patchMod);

                mods.Add(newMod);

                int newCount = 0;
                int overrideCount = 0;

                foreach (var record in formKeySet)
                {
                    var modKey = record.FormKey.ModKey;

                    var recordType = record.GetType();



                    // TODO add form to mod as override
                    //newMod.Add(record);

                    if (modKey == patchModKey)
                    {
                        // this is an entirely new form

                        // TODO clear out all formLinks patchMod

                        newCount++;
                    }
                    else
                    {
                        // this overrides an existing form.

                        patchMod.Remove(record);

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
