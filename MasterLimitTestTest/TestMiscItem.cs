using Mutagen.Bethesda;

namespace MasterLimitTestTest
{
    /// <summary>
    /// so that we know that this is a misc item.
    /// </summary>
    internal record TestMiscItem(IMajorRecordCommonGetter TheItem)
    {
        public FormKey FormKey => TheItem.FormKey;
    }
}
