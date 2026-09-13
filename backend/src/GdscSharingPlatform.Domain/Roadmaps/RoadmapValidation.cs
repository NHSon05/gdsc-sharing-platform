using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GdscSharingPlatform.Domain.Roadmaps;

public static class RoadmapValidation
{
    public static string RequiredText(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        value = value.Trim();
        if (value.Length > maxLength)
        {
            throw new ArgumentException($"Value must not exceed {maxLength} characters.", parameterName);
        }

        return value;
    }

    public static string Slug(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
        var text = new StringBuilder();
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                text.Append(character);
            }
        }

        return RequiredText(Regex.Replace(text.ToString(), "[^a-z0-9]+", "-").Trim('-'), 150, nameof(value));
    }

    public static Guid RequiredId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", parameterName);
        }

        return value;
    }

    public static int SortOrder(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        return value;
    }

    public static T DefinedEnum<T>(T value) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Unknown enum value.");
        }

        return value;
    }
}
