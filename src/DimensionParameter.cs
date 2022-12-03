using System;
using System.Text.RegularExpressions;

namespace InteractiveSelect;

internal interface IDimensionValue
{
    int CalculateAbsoluteValue(int referenceDimension);
}

internal class AbsoluteDimensionValue : IDimensionValue
{
    private readonly int absoluteValue;

    public AbsoluteDimensionValue(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException("Dimension can't be negative");
        this.absoluteValue = value;
    }

    public int CalculateAbsoluteValue(int referenceDimension)
        => absoluteValue;
}

internal class RelativeDimensionValue : IDimensionValue
{
    private readonly int percentage;

    public RelativeDimensionValue(int value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException($"Percentage must be a value between 0% and 100%.");
        this.percentage = value;
    }

    public int CalculateAbsoluteValue(int referenceDimension)
        => (referenceDimension * percentage) / 100;
}

public partial class DimensionParameter
{
    internal IDimensionValue Value { get; }

    public DimensionParameter(int absoluteValue)
    {
        Value = new AbsoluteDimensionValue(absoluteValue);
    }

    public DimensionParameter(string absoluteOrRelativeValue)
    {
        if (TryParsePercentage(absoluteOrRelativeValue, out int percentage))
        {
            Value = new RelativeDimensionValue(percentage);
        }
        else if (int.TryParse(absoluteOrRelativeValue, out var absoluteValue))
        {
            Value = new AbsoluteDimensionValue(absoluteValue);
        }
        else
        {
            throw new ArgumentException(
                $"Invalid value '{absoluteOrRelativeValue}'."
                + " Allowed values are absolute size (i.e. 41) or percentage (e.g. 73%).");
        }
    }

    [GeneratedRegex(@"(\d+)%")]
    private static partial Regex PercentRegex();

    private static bool TryParsePercentage(string input, out int result)
    {
        var match = PercentRegex().Match(input);
        if (match.Success)
        {
            result = int.Parse(match.Groups[1].Value);
            return true;
        }

        result = 0;
        return false;
    }
}
