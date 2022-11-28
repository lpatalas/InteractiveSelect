using System.Text;

namespace InteractiveSelect;

internal static class StringExtensions
{
    public static string RemoveControlCharacters(this string input)
    {
        int? firstControlCharIndex = null;

        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsControl(input[i]))
            {
                firstControlCharIndex = i;
                break;
            }
        }

        if (firstControlCharIndex.HasValue)
        {
            var result = new StringBuilder();
            result.Append(input, 0, firstControlCharIndex.Value);
            for (int i = firstControlCharIndex.Value + 1; i < input.Length; i++)
            {
                if (!char.IsControl(input[i]))
                    result.Append(input[i]);
            }

            return result.ToString();
        }
        else
        {
            return input;
        }
    }
}
