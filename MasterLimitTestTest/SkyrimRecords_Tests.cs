using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;

namespace MasterLimitTestTest
{
    public class SkyrimSERecords_Tests : SkyrimRecords_Tests
    {
        public SkyrimSERecords_Tests() : base(SkyrimRelease.SkyrimSE) { }
    }

    public class SkyrimVRRecords_Tests : SkyrimRecords_Tests
    {
        public SkyrimVRRecords_Tests() : base(SkyrimRelease.SkyrimVR) { }
    }

    public abstract class SkyrimRecords_Tests : BaseTests<SkyrimMod>
    {
        protected readonly SkyrimRelease release;

        public SkyrimRecords_Tests(SkyrimRelease release) : base(new(patchModKey, release))
        {
            this.release = release;
        }

        protected override SkyrimMod NewMod(string modName, SkyrimMod template) => new(ModKey.FromNameAndExtension(modName), template.SkyrimRelease);

        internal override TestMiscItem NewMisc(SkyrimMod mod, string editorID) => new(mod.MiscItems.AddNew(editorID));

        internal override TestContainer NewContainer(SkyrimMod mod, string editorID) => new(mod.Containers.AddNew(editorID));

        internal override void AddToContainer(TestContainer container, TestMiscItem item)
        {
            (((IContainer)container.TheContainer).Items ??= new())
                .Add(new()
                {
                    Item = new()
                    {
                        Count = 1,
                        Item = ((IItemGetter)((IMiscItem)item.TheItem)).AsLink<IItemGetter>()
                    }
                });
        }

        internal override TestContainer AddAsOverride(SkyrimMod mod, TestContainer container)
        {
            return new(mod.Containers.GetOrAddAsOverride((IContainer)container.TheContainer));
        }

        internal override HashSet<FormKey> AddOneOfEachRecord(SkyrimMod mod)
        {
            HashSet<FormKey> addedRecords = new();

            List<Exception> exceptions = new();

            var modType = mod.GetType();

            foreach (var property in modType.GetProperties())
            {
                var valueType = property.PropertyType;
                if (!valueType.IsGenericType) continue;

                var genericTypeDefinition = valueType.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(Group<>))
                {
                    var group = property.GetValue(mod);
                    if (group is null) continue;

                    var groupType = valueType.GetGenericArguments()[0];

                    if (groupType.IsAbstract)
                    {
                        if (property.Name == "Globals")
                        {
                            var foo = new GlobalFloat(mod);
                            mod.Globals.Add(foo);
                            addedRecords.Add(foo.FormKey);
                        }
                        else if(property.Name == "GameSettings")
                        {
                            var foo = new GameSettingBool(mod);
                            mod.GameSettings.Add(foo);
                            addedRecords.Add(foo.FormKey);
                        }
                        else
                        {
                            exceptions.Add(new NotImplementedException($"{property.Name} is a group of an abstract type"));
                        }
                    }
                    else
                    {
                        var addNewMethod = valueType.GetMethod("AddNew", Type.EmptyTypes);
                        if (addNewMethod is null)
                        {
                            exceptions.Add(new NotImplementedException($"can't find a{property.Name}.AddNew()"));
                            continue;
                        }
                        object? result;
                        try
                        {
                            result = addNewMethod.Invoke(group, null);
                        }
                        catch (Exception e)
                        {
                            exceptions.Add(new NotImplementedException($"{property.Name}.AddNew(); failed", e));
                            continue;
                        }
                        if (result is IMajorRecordCommonGetter record)
                        {
                            addedRecords.Add(record.FormKey);
                        }
                        else
                        {
                            var resultType = result?.GetType().ToString() ?? "null";
                            exceptions.Add(new NotImplementedException($"{property.Name}.AddNew(); returned an unhandled type: {resultType}"));
                        }
                    }

                }
                else if (genericTypeDefinition == typeof(ListGroup<>))
                {
                    if (property.Name == "Cells")
                    {
                        var cells = mod.Cells;

                        CellBlock cellBlock = new()
                        {
                            BlockNumber = 1,
                        };
                        cells.Records.Add(cellBlock);

                        CellSubBlock cellSubBlock = new()
                        {
                            BlockNumber = 1,
                        };
                        cellBlock.SubBlocks.Add(cellSubBlock);

                        Cell cell = new(mod);
                        cellSubBlock.Cells.Add(cell);
                        addedRecords.Add(cell.FormKey);

                        var persistentRefs = cell.Persistent;

                        PlacedNpc npc = new(mod);
                        persistentRefs.Add(npc);
                        addedRecords.Add(npc.FormKey);

                        PlacedObject obj = new(mod);
                        persistentRefs.Add(obj);
                        addedRecords.Add(obj.FormKey);
                    }
                    else
                    {
                        exceptions.Add(new NotImplementedException($"mod.{property.Name} is an unhandled type: {valueType} {genericTypeDefinition}"));
                    }
                }
                else
                {
                    //exceptions.Add(new NotImplementedException($"mod.{property.Name} is an unhandled type: {valueType} {genericTypeDefinition}"));
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException("",exceptions);
            }

            return addedRecords;
        }
    }
}
