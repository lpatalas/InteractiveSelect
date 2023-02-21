using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace InteractiveSelect;

public readonly struct Keystroke : IEquatable<Keystroke>
{
    private readonly ConsoleKey key;
    private readonly ConsoleModifiers modifiers;

    public Keystroke(ConsoleKey key, ConsoleModifiers modifiers)
    {
        this.key = key;
        this.modifiers = modifiers;
    }

    public static Keystroke Parse(string input)
    {
        ConsoleModifiers modifiers = default;
        int chunkStart = 0;
        var plusSignIndex = input.IndexOf('+');
        while (plusSignIndex > 0)
        {
            var modifierText = input.AsSpan(chunkStart, plusSignIndex).Trim();
            if (!Enum.TryParse<ConsoleModifiers>(modifierText, out var modifier))
                throw new ArgumentException($"Keystroke '{input}' is invalid because '{modifierText}' is not a valid modifier");

            modifiers |= modifier;

            chunkStart = plusSignIndex + 1;
            plusSignIndex = input.IndexOf("+", chunkStart);
        }

        ReadOnlySpan<char> keyText = input.AsSpan(chunkStart).Trim();
        if (!Enum.TryParse<ConsoleKey>(keyText, out var key))
            throw new ArgumentException($"Keystroke '{input}' is invalid because '{keyText}' is not a valid key");

        return new Keystroke(key, modifiers);
    }

    public bool IsPressed(ConsoleKeyInfo keyInfo)
        => keyInfo.Key == key && keyInfo.Modifiers == modifiers;

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is Keystroke other && Equals(other);

    public bool Equals(Keystroke other)
        => key == other.key && modifiers == other.modifiers;

    public override int GetHashCode()
        => HashCode.Combine(key, modifiers);

    public override string ToString()
    {
        var result = new StringBuilder();
        if (modifiers.HasFlag(ConsoleModifiers.Control))
            result.Append("Control+");
        if (modifiers.HasFlag(ConsoleModifiers.Shift))
            result.Append("Shift+");
        if (modifiers.HasFlag(ConsoleModifiers.Alt))
            result.Append("Alt+");
        result.Append(key);
        return result.ToString();
    }
}
