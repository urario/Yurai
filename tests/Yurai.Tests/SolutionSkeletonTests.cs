using System.Reflection;
using Xunit;

namespace Yurai.Tests;

public sealed class SolutionSkeletonTests
{
    [Fact]
    public void LibraryAssemblyIsLoadable()
    {
        Assembly assembly = Assembly.Load("Yurai");

        Assert.Equal("Yurai", assembly.GetName().Name);
    }
}
