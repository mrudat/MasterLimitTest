using MasterLimitTest;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MasterLimitTestTest
{

    public abstract partial class BaseTests<T>
        where T : IMod
    {
        protected static readonly ModKey patchModKey = ModKey.FromNameAndExtension("Synthesis.esp");

        protected T PatchMod { get; init; }

        protected readonly CustomSetFactory<ModKey> setFactory;

        protected readonly Action<T, IModContext<IMajorRecordCommonGetter>> addContextToMod;

        protected BaseTests(T patchMod, Action<T, IModContext<IMajorRecordCommonGetter>> addContextToMod)
        {
            this.PatchMod = patchMod;
            this.addContextToMod = addContextToMod;
            setFactory = new();
        }

        protected abstract T NewMod(string modName, T template);

        internal abstract TestMiscItem NewMisc(T mod, string editorID);

        internal abstract TestContainer NewContainer(T mod, string editorID);

        internal abstract void AddToContainer(TestContainer container, TestMiscItem item);

        internal abstract TestContainer AddAsOverride(T mod, TestContainer container);

        internal abstract HashSet<FormKey> AddOneOfEachRecord(T mod);
    }
}
