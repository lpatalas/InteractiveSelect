using System;

namespace InteractiveSelect;

internal enum EscapeSequenceCode
{
    Unknown,
    Sgr
}

internal readonly ref struct EscapeSequence
{
    private readonly ReadOnlySpan<char> chars;

    public static EscapeSequence ShowCursor => new EscapeSequence($"\x1b[?25h");
    public static EscapeSequence HideCursor => new EscapeSequence($"\x1b[?25l");
    public static EscapeSequence CursorUp(int cells) => new EscapeSequence($"\x1b[{cells}A");

    public EscapeSequenceCode Code => chars switch
    {
        [.., 'm'] => EscapeSequenceCode.Sgr,
        _ => EscapeSequenceCode.Unknown
    };

    public int Length => chars.Length;

    private EscapeSequence(ReadOnlySpan<char> chars)
    {
        this.chars = chars;
    }

    public static EscapeSequence Parse(ReadOnlySpan<char> input)
    {
        if (input.Length == 0 || input[0] != '\x1b')
            return new EscapeSequence(ReadOnlySpan<char>.Empty);

        if (input.Length < 2 || input[1] < 0x40 || input[1] > 0x5f)
            return new EscapeSequence(input.Slice(0, 1));

        switch (input[1])
        {
            // CSI sequence
            case '[':
                {
                    var i = 2; // Skip leading "\x1b["

                    while (i < input.Length && (input[i] < '\x40' || input[i] > '\x7e'))
                    {
                        i++;
                    }

                    if (i < input.Length)
                        i++; // Skip sequence terminator

                    return new EscapeSequence(input.Slice(0, i));
                }

            // Sequences terminated by ST or BEL
            case ']': // Operating System Command (OSC)
            case 'P': // Device Control String (DCS)
            case 'X': // Start of String
            case '^': // Privacy Message
            case '_': // Application Program Command
                {
                    var i = 2;

                    while (i < input.Length
                        && input[i] != '\b'
                        && !(i < input.Length - 1 && input[i] == '\x1b' && input[i + 1] == '\\'))
                    {
                        i++;
                    }

                    if (input[i] == '\b')
                        i++;
                    if (i < input.Length - 1 && input[i] == '\x1b' && input[i + 1] == '\\')
                        i += 2;

                    return new EscapeSequence(input.Slice(0, i));
                }

            default: // Not supported - take just ESC character, leave rest as is
                return new EscapeSequence(input.Slice(0, 1));
        }
    }

    public ReadOnlySpan<char> AsSpan()
        => chars;

    public override string ToString()
        => chars.ToString();

    public static implicit operator string(EscapeSequence seq)
        => seq.ToString();
}
