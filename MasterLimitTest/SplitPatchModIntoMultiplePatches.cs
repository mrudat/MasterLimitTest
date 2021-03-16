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
        internal record NewStruct(
            PropertyInfo Property,
            string ContainerName,
            MethodInfo Method
        );

        public static List<T> SplitPatchModIntoMultiplePatches<T>(
            T patchMod,
            List<PatchContents> patches,
            Func<string, T, T> NewMod,
            Action<T, IModContext<IMajorRecordCommonGetter>> addContextToMod)
            where T : IMod
        {
            List<T> mods = new();
            var patchModKey = patchMod.ModKey;

            // break here to investigate the results.
            var firstPatch = patches[0];
            patches.RemoveAt(0);

            int modCount = 0;

            mods.Add(patchMod);

            Dictionary<Type, NewStruct> properties = new();

            {
                var modType = patchMod.GetType();

                HashSet<Type> group = new();

                group.Add(typeof(Mutagen.Bethesda.Skyrim.Group<>));
                group.Add(typeof(Mutagen.Bethesda.Oblivion.Group<>));

                foreach (var property in modType.GetProperties())
                {
                    var groupType = property.PropertyType;
                    if (!groupType.IsGenericType) continue;

                    var genericTypeDefinition = groupType.GetGenericTypeDefinition();

                    string containerName = genericTypeDefinition.Name;

                    if (group.Contains(genericTypeDefinition))
                        containerName = "Group";

                    var valueType = groupType.GetGenericArguments()[0];

                    var method = groupType.GetMethod("Add", new[] { valueType });
                    if (method is null) continue;

                    properties[valueType] = new(property, containerName, method);
                }
            }

            void AddRecordToMod(T mod, IMajorRecordCommonGetter record)
            {
                var recordType = record.GetType();

                NewStruct? data = null;

                while (recordType is not null)
                {
                    if (properties.TryGetValue(recordType, out data))
                        break;

                    recordType = recordType.BaseType;
                }

                if (recordType is null || data is null)
                    throw new NotImplementedException($"{record.GetType()}");

                var property = data.Property;

                var containerName = data.ContainerName;

                if (containerName == "Group")
                {
                    var group = property.GetValue(mod);

                    data.Method.Invoke(group, new[] { record });
                }
                else
                {
                    throw new NotImplementedException($"mod.{property.Name} {containerName}");
                }

            }

            foreach (var patchContents in patches)
            {
                T newMod = NewMod($"Synthesis_{modCount}.esp", patchMod);

                mods.Add(newMod);

                int newCount = 0;
                int overrideCount = 0;

                foreach (var record in patchContents.records)
                {
                    var modKey = record.FormKey.ModKey;

                    AddRecordToMod(newMod, record);

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

                foreach (var context in patchContents.contexts)
                {
                    var modKey = context.ModKey;

                    addContextToMod(newMod, context);

                    if (modKey == patchModKey)
                    {
                        // this is an entirely new form

                        // TODO clear out all formLinks patchMod

                        newCount++;
                    }
                    else
                    {
                        // this overrides an existing form.

                        patchMod.Remove(context.Record);

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
