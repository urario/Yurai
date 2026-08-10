namespace Yurai.Benchmarks;

internal static class GraphFactory
{
    internal static Traced BuildBalanced(int leafCount)
    {
        if (leafCount <= 0 || (leafCount & (leafCount - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(leafCount), "Leaf count must be a positive power of two.");
        }

        var level = new Traced[leafCount];
        for (int index = 0; index < leafCount; index++)
        {
            level[index] = Traced.Of(index + 1m, $"Input{index}");
        }

        while (level.Length > 1)
        {
            var next = new Traced[level.Length / 2];
            for (int index = 0; index < level.Length; index += 2)
            {
                next[index / 2] = level[index] + level[index + 1];
            }

            level = next;
        }

        return level[0];
    }

    internal static Traced BuildSharedDiamond(int depth)
    {
        Traced result = Traced.Of(1m, "Root");
        for (int index = 0; index < depth; index++)
        {
            result += result;
        }

        return result;
    }
}
