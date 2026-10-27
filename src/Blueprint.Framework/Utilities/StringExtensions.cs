using System.Globalization;
using System.Text;

namespace Blueprint.Framework.Utilities;

public static class StringExtensions
{
    /// <summary>
    /// Lower-case ASCII slug: "João da Silva" becomes "joao.da.silva". Useful to build valid e-mail addresses or
    /// file names from arbitrary text.
    /// </summary>
    public static string ToSlug(this string value, char separator = '.')
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
            else if (builder.Length > 0 && builder[^1] != separator)
            {
                builder.Append(separator);
            }
        }

        return builder.ToString().Trim(separator);
    }
}
