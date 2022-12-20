using System.Diagnostics;
using System.Text;
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

        var testedListView = LoadListViewFromAsciiArt(testCase.Before);
        var expectedListView = LoadListViewFromAsciiArt(testCase.After);

        // TODO: Fix parsing when "After" list view has less items than original
        //Trace.Assert(testedListView.OriginalItems.Count == expectedListView.OriginalItems.Count);

        Trace.Assert(
            testedListView.PageSize == expectedListView.PageSize,
            "'Before' and 'After' list views should have the same page size");

        testCase.Action(testedListView);

        if (testedListView.ScrollOffset != expectedListView.ScrollOffset
            || testedListView.HighlightedIndex != expectedListView.HighlightedIndex
            || !testedListView.Items.SequenceEqual(expectedListView.Items))
        {
            var actualResult = SaveListViewAsAsciiArt(testedListView);

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
            testedListView.RaisedOnChangeNotifications.Should().BeEmpty();
        }

        if (testCase.ExpectedChangeNotifications.Any())
        {
            testedListView.RaisedOnChangeNotifications
                .Should().BeEquivalentTo(testCase.ExpectedChangeNotifications);
        }
    }

    private class TestListView<T> : ListView<T>
        where T : class
    {
        private readonly List<T?> raisedOnChangeNotifications = new List<T?>();
        public IReadOnlyList<T?> RaisedOnChangeNotifications => raisedOnChangeNotifications;

        public TestListView(IReadOnlyList<T> originalItems, int scrollOffset, int pageSize, int? highlightedIndex, ListFilterPredicate<T> filterPredicate)
            : base(originalItems, scrollOffset, pageSize, highlightedIndex, filterPredicate)
        {
        }

        protected override void OnHighlightedItemChanged(T? item)
        {
            raisedOnChangeNotifications.Add(item);
        }
    }

    private static TestListView<string> LoadListViewFromAsciiArt(string inputText)
    {
        var lines = inputText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var items = lines.Select(line => line.Trim(' ', '>', '|')).ToArray();
        var highlightedIndex = Array.FindIndex(lines, line => line.StartsWith(">"));
        var scrollOffset = Array.FindIndex(lines, line => line.EndsWith("|"));
        var pageSize = lines.Count(line => line.EndsWith("|"));

        return new TestListView<string>(
            items,
            scrollOffset,
            pageSize,
            highlightedIndex,
            filterPredicate: (item, filter) => item.Contains(filter));
    }

    private static string SaveListViewAsAsciiArt(ListView<string> listView)
    {
        var result = new StringBuilder();
        for (int i = 0; i < listView.OriginalItems.Count; i++)
        {
            if (i == listView.HighlightedIndex)
                result.Append("> ");
            else
                result.Append("  ");

            result.Append(listView.OriginalItems[i]);

            if (i >= listView.ScrollOffset && i < listView.ScrollOffset + listView.PageSize)
                result.Append(" |");

            result.AppendLine();
        }

        return result.ToString();
    }
}
