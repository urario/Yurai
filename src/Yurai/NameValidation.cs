namespace Yurai;

internal static class NameValidation
{
    internal static string Validate(string name)
    {
        return Validate(name, nameof(name), "name");
    }

    internal static string Validate(string name, string parameterName, string description)
    {
        if (name is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                $"A {description} cannot be empty or consist only of white-space characters.",
                parameterName);
        }

        return name;
    }
}
