using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Yurai.Tests")]

namespace Yurai;

// Temporary mutation-testing target until production behavior lands in Phase 2.
internal static class MutationTestingProbe
{
    internal static bool Negate(bool value) => !value;
}
