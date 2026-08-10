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

        if (!IsWellFormedUtf16(text))
        {
            throw new ArgumentException(
                $"A {parameterName} must contain well-formed UTF-16 text.",
                parameterName);
        }

        return text;
    }

    private static bool IsWellFormedUtf16(string text)
    {
        for (int index = 0; index < text.Length; index++)
        {
            char character = text[index];
            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= text.Length || !char.IsLowSurrogate(text[index + 1]))
                {
                    return false;
                }

                index++;
            }
            else if (char.IsLowSurrogate(character))
            {
                return false;
            }
        }

        return true;
    }
}
