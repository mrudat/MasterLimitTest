﻿using MasterLimitTest;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Skyrim;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MasterLimitTestTest
{

    public abstract partial class BaseTests<TMod, TModGetter>
        where TMod : class, IContextMod<TMod, TModGetter>, TModGetter
        where TModGetter : class, IContextGetterMod<TMod, TModGetter>
    {

        [Fact]
        public void TestAssert()
        {
            var mod1 = NewMod("Master1.esm", PatchMod);
            var mod1_record = NewMisc(mod1, "mod1_record");

            var mod2 = NewMod("Master2.esm", PatchMod);
            var mod2_record = NewMisc(mod2, "mod2_record");

            var mod3 = NewMod("Master3.esm", PatchMod);
            var mod3_record = NewMisc(mod3, "mod3_record");

            var recordWithTooManyMasters = NewContainer(PatchMod, "recordWithTooManyMasters");

            AddToContainer(recordWithTooManyMasters, mod1_record);
            AddToContainer(recordWithTooManyMasters, mod2_record);
            AddToContainer(recordWithTooManyMasters, mod3_record);

            // override this to reduce the size of test cases.
            var MAXIMUM_MASTERS_PER_MOD = 2;


            Assert.Throws<RecordException>(() => Program.ClassifyRecordsByReferencedMasters<TMod, TModGetter>(PatchMod, setFactory, MAXIMUM_MASTERS_PER_MOD));
        }

        [Fact]
        public void SingleNewRecord()
        {
            var newRecord = NewContainer(PatchMod, "newRecord");

            
            var recordSets = Program.ClassifyRecordsByReferencedMasters<TMod, TModGetter>(PatchMod, setFactory);


            Assert.Single(recordSets);

            var temp = setFactory.NewSet();

            temp.Add(patchModKey);

            Assert.True(recordSets.TryGetValue(temp.ToCustomSet(), out var newRecordData));

            Assert.Equal(1, newRecordData?.MasterCount);
            Assert.Equal(1, newRecordData?.contextSet.Count);
            Assert.Contains(newRecord.FormKey, newRecordData?.contextSet.Select(x => x.Record.FormKey));
            Assert.True(newRecordData?.hasNewRecords);


            var patches = Program.PatchesFromRecordSets(recordSets, setFactory);


            Assert.Single(patches);
            Assert.Contains(newRecord.FormKey, patches.Single().Select(x => x.Record.FormKey));
        }

        [Fact]
        public void SingleOverriddenRecord()
        {
            var newMod = NewMod("newMod.esp", PatchMod);

            var record = NewContainer(newMod, "newRecord");

            var overriddenRecord = AddAsOverride(PatchMod, record);


            var recordSets = Program.ClassifyRecordsByReferencedMasters<TMod, TModGetter>(PatchMod, setFactory);


            Assert.Single(recordSets);

            var temp = setFactory.NewSet();

            temp.Add(newMod.ModKey);

            Assert.True(recordSets.TryGetValue(temp.ToCustomSet(), out var newRecordData));

            Assert.Equal(1, newRecordData?.MasterCount);
            Assert.Equal(1, newRecordData?.contextSet.Count);
            Assert.Contains(overriddenRecord.FormKey, newRecordData?.contextSet.Select(x => x.Record.FormKey));
            Assert.False(newRecordData?.hasNewRecords);


            var patches = Program.PatchesFromRecordSets(recordSets, setFactory);


            Assert.Single(patches);
            Assert.Contains(overriddenRecord.FormKey, patches.Single().Select(x => x.Record.FormKey));
        }

        [Fact]
        public void TestTwoNewRecords()
        {
            var newRecord1 = NewContainer(PatchMod, "single_record");
            var newRecord2 = NewContainer(PatchMod, "another_record");


            var recordSets = Program.ClassifyRecordsByReferencedMasters<TMod, TModGetter>(PatchMod, setFactory);


            Assert.Single(recordSets);

            var temp = setFactory.NewSet();

            temp.Add(patchModKey);

            Assert.True(recordSets.TryGetValue(temp.ToCustomSet(), out var newRecordData));

            Assert.Equal(1, newRecordData?.MasterCount);
            Assert.Equal(2, newRecordData?.contextSet.Count);
            var formKeys = newRecordData?.contextSet.Select(x => x.Record.FormKey).ToHashSet();
            Assert.Contains(newRecord1.FormKey, formKeys);
            Assert.Contains(newRecord2.FormKey, formKeys);
            Assert.True(newRecordData?.hasNewRecords);


            var patches = Program.PatchesFromRecordSets(recordSets, setFactory);


            Assert.Single(patches);
            formKeys = patches.Single().Select(x => x.Record.FormKey).ToHashSet();
            Assert.Contains(newRecord1.FormKey, formKeys);
            Assert.Contains(newRecord2.FormKey, formKeys);
        }

        [Fact]
        public void TestOneNewRecordAndOneOverride()
        {
            var mod1 = NewMod("Master1.esm", PatchMod);
            var mod1_record = NewContainer(mod1, "mod1_record");


            var newRecord = NewContainer(PatchMod, "single_record");
            var overriddenRecord = AddAsOverride(PatchMod, mod1_record);


            var recordSets = Program.ClassifyRecordsByReferencedMasters<TMod, TModGetter>(PatchMod, setFactory);


            Assert.Equal(2, recordSets.Count);

            var temp = setFactory.NewSet();

            temp.Add(patchModKey);

            Assert.True(recordSets.TryGetValue(temp.ToCustomSet(), out var newRecordData));

            Assert.Equal(1, newRecordData?.MasterCount);
            Assert.Equal(1, newRecordData?.contextSet.Count);
            Assert.Contains(newRecord.FormKey, newRecordData?.contextSet.Select(x => x.Record.FormKey));
            Assert.True(newRecordData?.hasNewRecords);

            temp.Clear();
            temp.Add(mod1.ModKey);

            Assert.True(recordSets.TryGetValue(temp.ToCustomSet(), out var overrideRecordData));

            Assert.Equal(1, overrideRecordData?.MasterCount);
            Assert.Equal(1, overrideRecordData?.contextSet.Count);
            Assert.Contains(overriddenRecord.FormKey, overrideRecordData?.contextSet.Select(x => x.Record.FormKey));
            Assert.False(overrideRecordData?.hasNewRecords);


            var patches = Program.PatchesFromRecordSets(recordSets, setFactory);


            Assert.Single(patches);
            var patch = patches.Single();
            var formKeys = patch.Select(x => x.Record.FormKey).ToHashSet();
            Assert.Contains(newRecord.FormKey, formKeys);
            Assert.Contains(overriddenRecord.FormKey, formKeys);
        }


        [Fact]
        public void TwoPatches()
        {
            var MAXIMUM_MASTERS_PER_MOD = 3;

            var master1 = NewMod("Master1.esm", PatchMod);
            var master1_record = NewMisc(master1, "master1_record");

            var master2 = NewMod("Master2.esm", PatchMod);
            var master2_record = NewMisc(master2, "master2_record");

            var master3 = NewMod("Master3.esm", PatchMod);
            var master3_record = NewMisc(master3, "master3_record");

            var newMod = NewMod("newMod.esp", PatchMod);

            var newMod_record = NewContainer(newMod, "newRecord");
            AddToContainer(newMod_record, master2_record);


            var newRecord = NewContainer(PatchMod, "newRecord");
            AddToContainer(newRecord, master1_record);
            AddToContainer(newRecord, master2_record);

            var overriddenRecord = AddAsOverride(PatchMod, newMod_record);
            AddToContainer(overriddenRecord, master3_record);


            var recordSets = Program.ClassifyRecordsByReferencedMasters<TMod, TModGetter>(PatchMod, setFactory, MAXIMUM_MASTERS_PER_MOD);


            Assert.Equal(2, recordSets.Count);

            var temp = setFactory.NewSet();

            temp.Add(patchModKey);
            temp.Add(master1.ModKey);
            temp.Add(master2.ModKey);

            Assert.True(recordSets.TryGetValue(temp.ToCustomSet(), out var newRecordData));

            Assert.Equal(3, newRecordData?.MasterCount);
            Assert.Equal(1, newRecordData?.contextSet.Count);
            Assert.Contains(newRecord.FormKey, newRecordData?.contextSet.Select(x => x.Record.FormKey));
            Assert.True(newRecordData?.hasNewRecords);

            temp.Clear();
            temp.Add(newMod.ModKey);
            temp.Add(master2.ModKey);
            temp.Add(master3.ModKey);

            Assert.True(recordSets.TryGetValue(temp.ToCustomSet(), out var overrideRecordData));

            Assert.Equal(3, overrideRecordData?.MasterCount);
            Assert.Equal(1, overrideRecordData?.contextSet.Count);
            Assert.Contains(overriddenRecord.FormKey, overrideRecordData?.contextSet.Select(x => x.Record.FormKey));
            Assert.False(overrideRecordData?.hasNewRecords);


            var patches = Program.PatchesFromRecordSets(recordSets, setFactory, MAXIMUM_MASTERS_PER_MOD);


            Assert.Equal(2, patches.Count);

            HashSet<FormKey> allRecords = new();

            foreach (var patch in patches)
                allRecords.UnionWith(patch.Select(x => x.Record.FormKey));

            Assert.Contains(newRecord.FormKey, allRecords);
            Assert.Contains(overriddenRecord.FormKey, allRecords);
        }


        [Fact]
        public void TestCopyOneOfEachRecord()
        {
            var addedRecords = AddOneOfEachRecord(PatchMod);

            var linkCache = PatchMod.ToUntypedImmutableLinkCache();

            var addedContextRecords = PatchMod.EnumerateMajorRecordContexts<IMajorRecordCommon, IMajorRecordCommonGetter>(linkCache, false).Select(x => x.Record.FormKey).ToHashSet();


            var recordSets = Program.ClassifyRecordsByReferencedMasters<TMod, TModGetter>(PatchMod, setFactory);


            HashSet<FormKey> allRecords = new();
            foreach (var recordSet in recordSets.Values)
                allRecords.UnionWith(recordSet.contextSet.Select(x => x.Record.FormKey));

            Assert.Equal(addedContextRecords.Count, allRecords.Count);
            Assert.Equal(addedContextRecords, allRecords);


            var patches = Program.PatchesFromRecordSets(recordSets, setFactory);

            // copy all records from PatchMod to new patch.
            patches.Insert(0, new());

            allRecords.Clear();
            foreach (var patch in patches)
                allRecords.UnionWith(patch.Select(x => x.Record.FormKey));

            Assert.Equal(addedContextRecords.Count, allRecords.Count);
            Assert.Equal(addedContextRecords, allRecords);


            List<TMod> mods = Program.SplitPatchModIntoMultiplePatches(PatchMod, patches, NewMod);


            allRecords.Clear();

            foreach (var mod in mods)
                foreach (var record in mod.EnumerateMajorRecords())
                    allRecords.Add(record.FormKey);

            Assert.Equal(addedRecords.Count, allRecords.Count);
            Assert.Equal(addedRecords, allRecords);
        }
    }
}
