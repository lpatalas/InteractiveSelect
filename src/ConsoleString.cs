using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace InteractiveSelect;

internal readonly struct ConsoleString
{
    private readonly bool hasEscapeSequences;
    private readonly string value;

    private ConsoleString(string value, bool hasEscapeSequences)
    {
        this.hasEscapeSequences = hasEscapeSequences;
        this.value = value;
    }

    public static ConsoleString AsPlainText(string input)
    {
        return new ConsoleString(
            input.RemoveControlSequences(),
            hasEscapeSequences: false);
    }

    public ConsoleString AsColorizedString(string input)
    {
        return new ConsoleString(
            input.RemoveControlSequences(keepSgrSequences: true),
            hasEscapeSequences: true);
    }
}
