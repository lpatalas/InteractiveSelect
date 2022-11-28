using System.Text;

namespace InteractiveSelect;

internal static class StringExtensions
{
    public static string RemoveControlCharacters(this string input)
    {
        var hasControlCharacters = false;
        foreach (var c in input)
        {
            if (char.IsControl(c))
            {
                hasControlCharacters = true;
                break;
            }
        }

        if (hasControlCharacters)
        {
            var builder = new StringBuilder(capacity: input.Length);
            foreach (var c in input)
            {
                if (!char.IsControl(c))
                    builder.Append(c);
            }

            return builder.ToString();
        }
        else
        {
            return input;
        }
    }
}
