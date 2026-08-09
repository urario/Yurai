namespace Yurai;

internal static class NameValidation
{
    internal static string Validate(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A name cannot be empty or consist only of white-space characters.", nameof(name));
        }

        return name;
    }
}
