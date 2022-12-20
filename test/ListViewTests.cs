using System.Diagnostics;
using System.Text;
using Xunit;

namespace InteractiveSelect.Tests;

public partial class ListViewTests
{
    // Test cases use ASCII art to define ListView visually, e.g.
    //
    //    ItemA
    //    ItemB |   <-- Vertical bars specify current page. First bar from top
    //  > ItemC |       determines scroll offset (1) and the numbers of bars
    //    ItemD |       specifies page size (3).
    //    ItemE
    //
    //  - ">" character in the first column marks currently highlighted item.
    //  - The rest of non-whitespace characters are item value.

    [Theory]
    [MemberData(nameof(HighlightNextItemScenarios))]
    public void HighlightNextItemTests(Scenario testCase)
        => RunScenario(testCase);

    public static TheoryData<Scenario> HighlightNextItemScenarios =>
        new()
        {
            new Scenario("Highlight next item on the same page")
            {
                Before = """
                    > ItemA |
                      ItemB |
                      ItemC
                    """,

                Action = listView =>
                    listView.HighlightNextItem(),

                After = """
                      ItemA |
                    > ItemB |
                      ItemC
                    """,

                ExpectedChangeNotifications = { "ItemB" }
            },
            new Scenario("Scroll down when the last item on the page was selected")
            {
                Before = """
                      ItemA |
                    > ItemB |
                      ItemC
                    """,

                Action = listView =>
                    listView.HighlightNextItem(),

                After = """
                      ItemA
                      ItemB |
                    > ItemC |
                    """,

                ExpectedChangeNotifications = { "ItemC" }
            },

            new Scenario("Do nothing when the last item is already selected")
            {
                Before = """
                      ItemA
                      ItemB |
                    > ItemC |
                    """,

                Action = listView =>
                    listView.HighlightNextItem(),

                After = """
                      ItemA
                      ItemB |
                    > ItemC |
                    """,

                ExpectNoChangeNotifications = true
            }
        };

    [Theory]
    [MemberData(nameof(HighlightPreviousItemTestCases))]
    public void HighlightPreviousItemTests(Scenario testCase)
        => RunScenario(testCase);

    public static TheoryData<Scenario> HighlightPreviousItemTestCases =>
        new()
        {
            new Scenario("Highlight previous item on the same page")
            {
                Before = """
                      ItemA |
                    > ItemB |
                      ItemC
                    """,

                Action = listView =>
                    listView.HighlightPreviousItem(),

                After = """
                    > ItemA |
                      ItemB |
                      ItemC
                    """,

                ExpectedChangeNotifications = { "ItemA" }
            },
            new Scenario("Scroll up when the first item on the page was selected")
            {
                Before = """
                      ItemA
                    > ItemB |
                      ItemC |
                    """,

                Action = listView =>
                    listView.HighlightPreviousItem(),

                After = """
                    > ItemA |
                      ItemB |
                      ItemC
                    """,

                ExpectedChangeNotifications = { "ItemA" }
            },

            new Scenario("Do nothing when the first item is already selected")
            {
                Before = """
                    > ItemA |
                      ItemB |
                      ItemC
                    """,

                Action = listView =>
                    listView.HighlightPreviousItem(),

                After = """
                    > ItemA |
                      ItemB |
                      ItemC
                    """,

                ExpectNoChangeNotifications = true
            }
        };

    [Theory]
    [MemberData(nameof(HighlightFirstItemTestCases))]
    public void HighlightFirstItemTests(Scenario testCase)
        => RunScenario(testCase);

    public static TheoryData<Scenario> HighlightFirstItemTestCases =>
        new()
        {
            new Scenario("Highlight first item when other one is highlighted")
            {
                Before = """
                      ItemA |
                      ItemB |
                    > ItemC |
                      ItemD
                    """,

                Action = listView =>
                    listView.HighlightFirstItem(),

                After = """
                    > ItemA |
                      ItemB |
                      ItemC |
                      ItemD
                    """,

                ExpectedChangeNotifications = { "ItemA" }
            },
            new Scenario("Scroll to the top when it's not visible on the current page")
            {
                Before = """
                      ItemA
                      ItemB |
                      ItemC |
                    > ItemD |
                    """,

                Action = listView =>
                    listView.HighlightFirstItem(),

                After = """
                    > ItemA |
                      ItemB |
                      ItemC |
                      ItemD
                    """,

                ExpectedChangeNotifications = { "ItemA" }
            },

            new Scenario("Do nothing when the first item is already selected")
            {
                Before = """
                    > ItemA |
                      ItemB |
                      ItemC |
                      ItemD
                    """,

                Action = listView =>
                    listView.HighlightFirstItem(),

                After = """
                    > ItemA |
                      ItemB |
                      ItemC |
                      ItemD
                    """,

                ExpectNoChangeNotifications = true
            }
        };

    [Theory]
    [MemberData(nameof(HighlightLastItemTestCases))]
    public void HighlightLastItemTests(Scenario testCase)
        => RunScenario(testCase);

    public static TheoryData<Scenario> HighlightLastItemTestCases =>
        new()
        {
            new Scenario("Highlight last item when other one is highlighted")
            {
                Before = """
                      ItemA 
                    > ItemB |
                      ItemC |
                      ItemD |
                    """,

                Action = listView =>
                    listView.HighlightLastItem(),

                After = """
                      ItemA
                      ItemB |
                      ItemC |
                    > ItemD |
                    """,

                ExpectedChangeNotifications = { "ItemD" }
            },
            new Scenario("Scroll to the bottom it's not visible on the current page")
            {
                Before = """
                    > ItemA |
                      ItemB |
                      ItemC |
                      ItemD
                    """,

                Action = listView =>
                    listView.HighlightLastItem(),

                After = """
                      ItemA
                      ItemB |
                      ItemC |
                    > ItemD |
                    """,

                ExpectedChangeNotifications = { "ItemD" }
            },

            new Scenario("Do nothing when the last item is already selected")
            {
                Before = """
                      ItemA
                      ItemB |
                      ItemC |
                    > ItemD |
                    """,

                Action = listView =>
                    listView.HighlightLastItem(),

                After = """
                      ItemA
                      ItemB |
                      ItemC |
                    > ItemD |
                    """,

                ExpectNoChangeNotifications = true
            }
        };

    [Theory]
    [MemberData(nameof(HighlightItemPageDownScenarios))]
    public void HighlightItemPageDownTests(Scenario testCase)
        => RunScenario(testCase);

    public static TheoryData<Scenario> HighlightItemPageDownScenarios =>
        new()
        {
            new Scenario("Should move highlight to last item on the page when the first one is highlighted")
            {
                Before = """
                    > ItemA |
                      ItemB |
                      ItemC |
                      ItemD
                    """,

                Action = listView =>
                    listView.HighlightItemPageDown(),

                After = """
                      ItemA |
                      ItemB |
                    > ItemC |
                      ItemD
                    """,

                ExpectedChangeNotifications = { "ItemC" }
            },
            new Scenario("Scroll down when the new item is outside the current page")
            {
                Before = """
                      ItemA |
                    > ItemB |
                      ItemC |
                      ItemD
                      ItemE
                    """,

                Action = listView =>
                    listView.HighlightItemPageDown(),

                After = """
                      ItemA
                      ItemB |
                      ItemC |
                    > ItemD |
                      ItemE
                    """,

                ExpectedChangeNotifications = { "ItemD" }
            },

            new Scenario("Should highlight last item when list is already scrolled to the end")
            {
                Before = """
                      ItemA
                      ItemB |
                    > ItemC |
                      ItemD |
                      ItemE |
                    """,

                Action = listView =>
                    listView.HighlightItemPageDown(),

                After = """
                      ItemA
                      ItemB |
                      ItemC |
                      ItemD |
                    > ItemE |
                    """,

                ExpectedChangeNotifications = { "ItemE" }
            },

            new Scenario("Do nothing when the last item is already selected")
            {
                Before = """
                      ItemA
                      ItemB |
                    > ItemC |
                    """,

                Action = listView =>
                    listView.HighlightItemPageDown(),

                After = """
                      ItemA
                      ItemB |
                    > ItemC |
                    """,

                ExpectNoChangeNotifications = true
            }
        };

    [Theory]
    [MemberData(nameof(HighlightItemPageUpScenarios))]
    public void HighlightItemPageUpTests(Scenario testCase)
        => RunScenario(testCase);

    public static TheoryData<Scenario> HighlightItemPageUpScenarios =>
        new()
        {
            new Scenario("Should move highlight to the first item on the page when the first one is highlighted")
            {
                Before = """
                      ItemA |
                      ItemB |
                    > ItemC |
                      ItemD
                    """,

                Action = listView =>
                    listView.HighlightItemPageUp(),

                After = """
                    > ItemA |
                      ItemB |
                      ItemC |
                      ItemD
                    """,

                ExpectedChangeNotifications = { "ItemA" }
            },
            new Scenario("Scroll up when the new item is outside the current page")
            {
                Before = """
                      ItemA
                      ItemB
                      ItemC |
                    > ItemD |
                      ItemE |
                    """,

                Action = listView =>
                    listView.HighlightItemPageUp(),

                After = """
                      ItemA
                    > ItemB |
                      ItemC |
                      ItemD |
                      ItemE
                    """,

                ExpectedChangeNotifications = { "ItemB" }
            },

            new Scenario("Should highlight first item when list is already scrolled to the end")
            {
                Before = """
                      ItemA |
                      ItemB |
                    > ItemC |
                      ItemD |
                      ItemE
                    """,

                Action = listView =>
                    listView.HighlightItemPageUp(),

                After = """
                    > ItemA |
                      ItemB |
                      ItemC |
                      ItemD |
                      ItemE
                    """,

                ExpectedChangeNotifications = { "ItemA" }
            },

            new Scenario("Do nothing when the first item is already selected")
            {
                Before = """
                    > ItemA |
                      ItemB |
                      ItemC
                    """,

                Action = listView =>
                    listView.HighlightItemPageUp(),

                After = """
                    > ItemA |
                      ItemB |
                      ItemC
                    """,

                ExpectNoChangeNotifications = true
            }
        };

    [Theory]
    [MemberData(nameof(FilteringScenarios))]
    public void FilteringScenarioTests(Scenario testCase)
        => RunScenario(testCase);

    public static TheoryData<Scenario> FilteringScenarios =>
        new()
        {
            new Scenario("Keep highlighted index as is if it's still visible")
            {
                Before = """
                      AAB |
                      AAC |
                    > BBB |
                      BCC
                    """,

                Action = listView =>
                    listView.Filter = "B",

                After = """
                      AAB |
                    > BBB |
                      BCC |
                    """,

                ExpectNoChangeNotifications = true
            },

            new Scenario("Move highlighted index to the first item if currently highlighted one was filtered-out")
            {
                Before = """
                      AAA |
                      ABB |
                    > BBB |
                      ACC
                    """,

                Action = listView =>
                    listView.Filter = "A",

                After = """
                    > AAA |
                      ABB |
                      ACC |
                    """,

                ExpectedChangeNotifications = { "AAA" }
            },

            new Scenario("Adjust scroll offset when highlighted item position change after filtering")
            {
                Before = """
                      AAA
                      ABB
                    > BBB |
                      ACC |
                      CCC |
                      CCB
                      CBD
                    """,

                Action = listView =>
                    listView.Filter = "B",

                After = """
                      ABB
                    > BBB |
                      CCB |
                      CBD |
                    """,

                ExpectNoChangeNotifications = true
            },
        };
}
