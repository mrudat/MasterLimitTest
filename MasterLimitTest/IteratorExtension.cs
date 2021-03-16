using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterLimitTest
{
    public static partial class IteratorExtension
    {
        public static readonly List<Type> typesWithContext = new()
        {
            typeof(Mutagen.Bethesda.Skyrim.IDialogTopic),
            typeof(Mutagen.Bethesda.Skyrim.ICellBlock),
            typeof(Mutagen.Bethesda.Skyrim.IWorldspaceGetter),
            typeof(Mutagen.Bethesda.Oblivion.ICellBlock),
            typeof(Mutagen.Bethesda.Oblivion.IWorldspaceGetter)
        };

        public static readonly Type groupInterface = typeof(IMajorRecordEnumerable);

        public static IEnumerable<IMajorRecordCommonGetter> EnumerateContextFreeMajorRecords(this IModGetter mod)
        {
            var modType = mod.GetType();

            // TODO

            foreach (var property in modType.GetProperties())
            {
                var valueType = property.PropertyType;
                if (!valueType.IsGenericType) continue;

                var genericTypeDefinition = valueType.GetGenericTypeDefinition();

                var itemType = genericTypeDefinition.GetGenericArguments()[0];

                if (typesWithContext.Any(x => x.IsAssignableFrom(itemType)))
                    continue;

                if (groupInterface.IsAssignableFrom(genericTypeDefinition))
                {
                    var group = property.GetValue(mod);
                    if (group is null) continue;

                    foreach (var record in ((IMajorRecordEnumerable)group).EnumerateMajorRecords())
                    {
                        yield return record;
                    }
                }
            }

            yield break;
        }

        public static IEnumerable<IModContext<IMajorRecordCommonGetter>> EnumerateOnlyMajorRecordsWithContexts(this IModGetter mod)
        {
            if (mod is ISkyrimModGetter skyrimMod)
            {
                return Enumerable
                    .Empty<ModContext<IMajorRecordCommonGetter>>()
                    .Concat(skyrimMod.Cells.EnumerateMajorRecordContexts(skyrimMod.ModKey))
                    .Concat(skyrimMod.Worldspaces.EnumerateMajorRecordContexts(skyrimMod.ModKey))
                    .Concat(skyrimMod.DialogTopics.EnumerateMajorRecordContexts(skyrimMod.ModKey));
                // TODO
            }
            else if (mod is IOblivionModGetter oblivionMod)
            {
                return Enumerable
                    .Empty<ModContext<IMajorRecordCommonGetter>>()
                    .Concat(oblivionMod.Cells.EnumerateMajorRecordContexts(oblivionMod.ModKey))
                    .Concat(oblivionMod.Worldspaces.EnumerateMajorRecordContexts(oblivionMod.ModKey));
                // TODO
            }
            else
            {
                throw new NotImplementedException("TODO");
            }
        }


        internal static IEnumerable<IModContext<IMajorRecordCommonGetter>> EnumerateMajorRecordContexts<T, TCell>(
            this TCell cell,
            ModKey modKey,
            ModContext<T> parent)
            where T : class
            where TCell : IMajorRecordCommonGetter, IMajorRecordGetterEnumerable
        {
            var cellContext = new ModContext<TCell>(
                modKey: modKey,
                parent: parent,
                record: cell);
            var yielded = false;
            foreach (var record in cell.EnumerateMajorRecords())
            {
                yielded = true;
                yield return new ModContext<IMajorRecordCommonGetter>(
                    modKey: modKey,
                    parent: cellContext,
                    record: record);
            }
            if (!yielded)
                yield return (IModContext<IMajorRecordCommonGetter>)cellContext;
        }

        internal static IEnumerable<IModContext<IMajorRecordCommonGetter>> EnumerateMajorRecordContexts<TWorldspace,TWorldspaceBlock,TWorldspaceSubBlock>(this IEnumerable<TWorldspace> worldSpaces, ModKey modKey)
            where TWorldspace : IMajorRecordCommonGetter
            where TWorldspaceBlock : class
            where TWorldspaceSubBlock : IMajorRecordGetterEnumerable
        {
            foreach (var worldSpace in worldSpaces)
            {
                var worldSpaceContext = new ModContext<TWorldspace>(
                    modKey: modKey,
                    parent: null,
                    record: worldSpace);

                IReadOnlyList<TWorldspaceBlock> subCells = (IReadOnlyList<TWorldspaceBlock>) worldSpace switch
                {
                    Mutagen.Bethesda.Skyrim.IWorldspaceGetter skyrimWorldspace => (IReadOnlyList<TWorldspaceBlock>)skyrimWorldspace.SubCells,
                    Mutagen.Bethesda.Oblivion.IWorldspaceGetter oblivionWorldspace => (IReadOnlyList<TWorldspaceBlock>)oblivionWorldspace.SubCells,
                    _ => throw new NotImplementedException(),
                };

                foreach (var block in subCells)
                {
                    var blockContext = new ModContext<TWorldspaceBlock>(
                        modKey: modKey,
                        parent: worldSpaceContext,
                        record: block);
                    foreach (var subBlock in block.Items)
                    {
                        var subBlockContext = new ModContext<TWorldspaceSubBlock>(
                            modKey: modKey,
                            parent: blockContext,
                            record: subBlock);
                        foreach (var cell in subBlock.Items)
                            foreach (var context in cell.EnumerateMajorRecordContexts(modKey, subBlockContext))
                                yield return context;
                    }
                }
            }
        }

    }
}
