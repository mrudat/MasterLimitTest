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

        protected override OblivionMod NewMod(string modName) => new(ModKey.FromNameAndExtension(modName));

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

            void addRecord<T>(Group<T> group)
                where T : MajorRecord, IOblivionMajorRecordInternal => addedRecords.Add(group.AddNew().FormKey);

            foreach (var item in Enum.GetValues<GroupTypeEnum>())
            {
                // TODO ?
                // Profit!
            }

            addRecord(mod.Activators);
            addRecord(mod.AIPackages);
            addRecord(mod.AlchemicalApparatus);
            addRecord(mod.Ammunitions);
            addRecord(mod.AnimatedObjects);
            addRecord(mod.Armors);
            addRecord(mod.Birthsigns);
            addRecord(mod.Books);
            addRecord(mod.Classes);
            addRecord(mod.Climates);
            addRecord(mod.Clothes);
            addRecord(mod.CombatStyles);
            addRecord(mod.Containers);
            addRecord(mod.Creatures);
            addRecord(mod.DialogTopics);
            addRecord(mod.Doors);
            addRecord(mod.EffectShaders);
            addRecord(mod.Enchantments);
            addRecord(mod.Eyes);
            addRecord(mod.Factions);
            addRecord(mod.Flora);
            addRecord(mod.Furniture);
            //addRecord(mod.GameSettings); // TODO
            //addRecord(mod.Globals); // TODO
            addRecord(mod.Grasses);
            addRecord(mod.Hairs);
            addRecord(mod.IdleAnimations);
            addRecord(mod.Ingredients);
            addRecord(mod.Keys);
            addRecord(mod.LandTextures);
            addRecord(mod.LeveledCreatures);
            addRecord(mod.LeveledItems);
            addRecord(mod.LeveledSpells);
            addRecord(mod.Lights);
            addRecord(mod.LoadScreens);
            addRecord(mod.MagicEffects);
            addRecord(mod.Miscellaneous);
            addRecord(mod.Npcs);

            // TODO

            // patchMod.Cells; //TODO

            return addedRecords;
        }
    }
}
