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

        protected override SkyrimMod NewMod(string modName) => new(ModKey.FromNameAndExtension(modName), release);

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

            void addRecord<T>(Group<T> group)
                where T : MajorRecord, ISkyrimMajorRecordInternal => addedRecords.Add(group.AddNew().FormKey);

            foreach (var item in Enum.GetValues<GroupTypeEnum>())
            {
                // TODO ?
                // Profit!
            }

            addRecord(mod.AcousticSpaces);

            // TODO

            return addedRecords;
        }
    }
}
