using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using System;
using System.Collections.Generic;

namespace MasterLimitTestTest
{
    /// <summary>
    /// This is pointless, but in lieu of Fallout4 support...
    /// </summary>
    // public class ClassifyFalloutRecords_Tests : ClassifyRecordsByReferencedMasters_Tests<IFalloutMod>
    public class OblivionRecords_Tests : BaseTests<OblivionMod>
    {
        public OblivionRecords_Tests() : base(new(patchModKey))
        {

        }

        protected override OblivionMod NewMod(string modName, OblivionMod template) => new(ModKey.FromNameAndExtension(modName));

        internal override TestMiscItem NewMisc(OblivionMod mod, string editorID) => new(mod.Miscellaneous.AddNew(editorID));

        internal override TestContainer NewContainer(OblivionMod mod, string editorID) => new(mod.Containers.AddNew(editorID));

        internal override void AddToContainer(TestContainer container, TestMiscItem item)
        {
            ((IContainer)container.TheContainer).Items
                .Add(new()
                {
                    Count = 1,
                    Item = ((IAItemGetter)((IMiscellaneous)item.TheItem)).AsLink<IAItemGetter>()
                });
        }

        internal override TestContainer AddAsOverride(OblivionMod mod, TestContainer container)
        {
            return new(mod.Containers.GetOrAddAsOverride((IContainer)container.TheContainer));
        }

        internal override HashSet<FormKey> AddOneOfEachRecord(OblivionMod mod)
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
                        else if (property.Name == "GameSettings")
                        {
                            var foo = new GameSettingFloat(mod);
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
                throw new AggregateException("", exceptions);
            }

            return addedRecords;
        }
    }
}
