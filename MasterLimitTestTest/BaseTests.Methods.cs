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

    public abstract partial class BaseTests<TMod, TModGetter>
        where TMod : class, IContextMod<TMod, TModGetter>, TModGetter
        where TModGetter : class, IContextGetterMod<TMod, TModGetter>
    {
        protected static readonly ModKey patchModKey = ModKey.FromNameAndExtension("Synthesis.esp");

        protected TMod PatchMod { get; init; }

        protected readonly CustomSetFactory<ModKey> setFactory;

        protected BaseTests(TMod patchMod)
        {
            this.PatchMod = patchMod;
            setFactory = new();
        }

        protected abstract TMod NewMod(string modName, TModGetter template);

        internal abstract TestMiscItem NewMisc(TMod mod, string editorID);

        internal abstract TestContainer NewContainer(TMod mod, string editorID);

        internal abstract void AddToContainer(TestContainer container, TestMiscItem item);

        internal abstract TestContainer AddAsOverride(TMod mod, TestContainer container);

        internal abstract HashSet<FormKey> AddOneOfEachRecord(TMod mod);
    }
}
