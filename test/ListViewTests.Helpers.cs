using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace InteractiveSelect.Tests;

partial class ListViewTests
{
    public record Scenario(string Name)
    {
        internal string Before { get; init; } = default!;
        internal string After { get; init; } = default!;
        internal Action<ListView<string>> Action { get; init; } = default!;
        internal IList<string> ExpectedChangeNotifications { get; init; } = new List<string>();
        internal bool ExpectNoChangeNotifications { get; init; }
    }

    private static void RunScenario(Scenario testCase)
    {
        Trace.Assert(
            !testCase.ExpectNoChangeNotifications || !testCase.ExpectedChangeNotifications.Any(),
            $"{nameof(testCase.ExpectNoChangeNotifications)} and {nameof(testCase.ExpectedChangeNotifications)} can't be both set at the same time");

        var testedListView = ListView<string>.FromAsciiArt(testCase.Before);
        var expectedListView = ListView<string>.FromAsciiArt(testCase.After);

        List<string?> raisedOnChangeNotifications = new List<string?>();
        testedListView.HighlightedItemChanged += (_, e) => raisedOnChangeNotifications.Add(e.Item);

        // TODO: Fix parsing when "After" list view has less items than original
        //Trace.Assert(testedListView.OriginalItems.Count == expectedListView.OriginalItems.Count);

        testCase.Action(testedListView);

        if (!testedListView.HasSameVisibleItems(expectedListView))
        {
            var actualResult = testedListView.ToAsciiArt();

            var failureMessage = $"""
                Expected list view to be:

                {testCase.After}

                but found:

                {actualResult}
                """;

            Assert.Fail(failureMessage);
        }

        if (testCase.ExpectNoChangeNotifications)
        {
            raisedOnChangeNotifications.Should().BeEmpty();
        }

        if (testCase.ExpectedChangeNotifications.Any())
        {
            raisedOnChangeNotifications
                .Should().BeEquivalentTo(testCase.ExpectedChangeNotifications);
        }
    }
}
