namespace Yurai;

internal static class ArgumentValidation
{
    internal static string Validate(string text, string parameterName)
    {
        if (text is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                $"A {parameterName} cannot be empty or consist only of white-space characters.",
                parameterName);
        }

        return text;
    }
}
