using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using System.Collections.Generic;

namespace MasterLimitTest
{
    public static partial class IteratorExtension
    {

        internal static IEnumerable<IModContext<IMajorRecordCommonGetter>> EnumerateMajorRecordContexts(
    this IGroupGetter<IWorldspaceGetter> worldSpaces,
    ModKey modKey)
        {
            foreach (var worldSpace in worldSpaces)
            {
                var worldSpaceContext = new ModContext<IWorldspaceGetter>(
                    modKey: modKey,
                    parent: null,
                    record: worldSpace);
                foreach (var block in worldSpace.SubCells)
                {
                    var blockContext = new ModContext<IWorldspaceBlockGetter>(
                        modKey: modKey,
                        parent: worldSpaceContext,
                        record: block);
                    foreach (var subBlock in block.Items)
                    {
                        var subBlockContext = new ModContext<IWorldspaceSubBlockGetter>(
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

        internal static IEnumerable<IModContext<IMajorRecordCommonGetter>> EnumerateMajorRecordContexts(
            this IListGroupGetter<ICellBlockGetter> cellBlocks,
            ModKey modKey)
        {
            foreach (var block in cellBlocks.Records)
            {
                var blockNum = block.BlockNumber;
                var blockContext = new ModContext<ICellBlockGetter>(
                    modKey: modKey,
                    parent: null,
                    record: block);
                foreach (var subBlock in block.SubBlocks)
                {
                    var subBlockNum = subBlock.BlockNumber;
                    var subBlockContext = new ModContext<ICellSubBlockGetter>(
                        modKey: modKey,
                        parent: blockContext,
                        record: subBlock);
                    foreach (var cell in subBlock.Cells)
                        foreach (var context in cell.EnumerateMajorRecordContexts(modKey, subBlockContext))
                            yield return context;
                }
            }
        }

        internal static IEnumerable<IModContext<IMajorRecordCommonGetter>> EnumerateMajorRecordContexts(
            this IGroupGetter<IDialogTopicGetter> dialogTopics,
            ModKey modKey)
        {
            foreach (var dialogTopic in dialogTopics)
            {
                var dialogTopicContext = new ModContext<IDialogTopicGetter>(
                    modKey: modKey,
                    parent: null,
                    record: dialogTopic
                    );
                var yielded = false;
                foreach (var dialogResponses in dialogTopic.Responses)
                {
                    yielded = true;
                    yield return new ModContext<IDialogResponsesGetter>(
                        modKey: modKey,
                        parent: dialogTopicContext,
                        record: dialogResponses
                        );
                }
                if (!yielded)
                    yield return dialogTopicContext;
            }
        }

    }
}
