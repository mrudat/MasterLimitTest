using Mutagen.Bethesda;

namespace MasterLimitTestTest
{
    /// <summary>
    /// so that we know that this is a container.
    /// </summary>
    internal record TestContainer(IMajorRecordCommonGetter TheContainer)
    {
        public FormKey FormKey => TheContainer.FormKey;
    }
}
