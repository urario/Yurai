using CsCheck;
using Xunit;

namespace Yurai.Tests;

public sealed class MutationTestingProbeProperties
{
    [Fact]
    [Trait("Category", "Property")]
    public void NegationProducesOppositeValue()
    {
        Gen.Bool.Sample(value => Assert.Equal(!value, MutationTestingProbe.Negate(value)));
    }
}
