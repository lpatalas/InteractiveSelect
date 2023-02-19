using Xunit;

namespace InteractiveSelect.Tests;

partial class ListViewTests
{
    [Theory]
    [MemberData(nameof(SelectionScenarios))]
    public void SelectionScenarioTests(Scenario scenario)
        => scenario.Run();

    public static TheoryData<Scenario> SelectionScenarios =>
        new()
        {
            new Scenario("Select highlighted item when toggling")
            {
                Before = """
                      AAB |
                    > AAC |
                      BBB |
                      BCC
                    """,

                Action = listView =>
                    listView.ToggleSelection(),

                After = """
                       AAB |
                    > *AAC |
                       BBB |
                       BCC
                    """
            },
            new Scenario("Unselect highlighted item when toggling")
            {
                Before = """
                       AAB |
                    > *AAC |
                       BBB |
                       BCC
                    """,

                Action = listView =>
                    listView.ToggleSelection(),

                After = """
                      AAB |
                    > AAC |
                      BBB |
                      BCC
                    """
            },
            new Scenario("Select all items")
            {
                Before = """
                       AAB |
                    > *AAC |
                       BBB |
                      *BCC
                    """,

                Action = listView =>
                    listView.SelectAll(),

                After = """
                      *AAB |
                    > *AAC |
                      *BBB |
                      *BCC
                    """
            },
            new Scenario("Unselect all items")
            {
                Before = """
                       AAB |
                    > *AAC |
                       BBB |
                      *BCC
                    """,

                Action = listView =>
                    listView.UnselectAll(),

                After = """
                      AAB |
                    > AAC |
                      BBB |
                      BCC
                    """
            },
            new Scenario("Invert selection")
            {
                Before = """
                       AAB |
                    > *AAC |
                       BBB |
                      *BCC
                    """,

                Action = listView =>
                    listView.InvertSelection(),

                After = """
                      *AAB |
                    >  AAC |
                      *BBB |
                       BCC
                    """
            },
            new Scenario("Unselect filtered out items")
            {
                Before = """
                      *AAB |
                    > *AAC |
                      *BBB |
                      *BCC
                    """,

                Action = listView =>
                {
                    listView.Filter = "A";
                    listView.Filter = string.Empty;
                },

                After = """
                      *AAB |
                    > *AAC |
                       BBB |
                       BCC
                    """
            },
            new Scenario("Select all should only select visible items")
            {
                Before = """
                      AAB |
                    > AAC |
                      BBB |
                      BCC
                    """,

                Action = listView =>
                {
                    listView.Filter = "A";
                    listView.SelectAll();
                    listView.Filter = string.Empty;
                },

                After = """
                      *AAB |
                    > *AAC |
                       BBB |
                       BCC
                    """
            },
            new Scenario("Invert selection should only toggle visible items")
            {
                Before = """
                       AAB |
                    > *AAC |
                       BBB |
                      *BCC
                    """,

                Action = listView =>
                {
                    listView.Filter = "A";
                    listView.InvertSelection();
                    listView.Filter = string.Empty;
                },

                After = """
                      *AAB |
                    >  AAC |
                       BBB |
                       BCC
                    """
            },
            new Scenario("Highlight previous item with selection should select first item")
            {
                Before = """
                    > AAB |
                      AAC |
                      BBB |
                      BCC
                    """,

                Action = listView =>
                {
                    listView.HighlightPreviousItem(toggleSelection: true);
                },

                After = """
                    > *AAB |
                       AAC |
                       BBB |
                       BCC
                    """
            },
            new Scenario("Highlight previous item with selection should unselect first item")
            {
                Before = """
                    > *AAB |
                       AAC |
                       BBB |
                       BCC
                    """,

                Action = listView =>
                {
                    listView.HighlightPreviousItem(toggleSelection: true);
                },

                After = """
                    > AAB |
                      AAC |
                      BBB |
                      BCC
                    """
            },
            new Scenario("Highlight previous item with selection should select item")
            {
                Before = """
                      AAB |
                    > AAC |
                      BBB |
                      BCC
                    """,

                Action = listView =>
                {
                    listView.HighlightPreviousItem(toggleSelection: true);
                },

                After = """
                    >  AAB |
                      *AAC |
                       BBB |
                       BCC
                    """,
            },
            new Scenario("Highlight previous item with selection should unselect item")
            {
                Before = """
                       AAB |
                    > *AAC |
                       BBB |
                       BCC
                    """,

                Action = listView =>
                {
                    listView.HighlightPreviousItem(toggleSelection: true);
                },

                After = """
                    > AAB |
                      AAC |
                      BBB |
                      BCC
                    """,
            },
            new Scenario("Highlight next item with selection should select last item")
            {
                Before = """
                      AAB 
                      AAC |
                      BBB |
                    > BCC |
                    """,

                Action = listView =>
                {
                    listView.HighlightNextItem(toggleSelection: true);
                },

                After = """
                       AAB 
                       AAC |
                       BBB |
                    > *BCC |
                    """,
            },
            new Scenario("Highlight next item with selection should unselect last item")
            {
                Before = """
                       AAB 
                       AAC |
                       BBB |
                    > *BCC |
                    """,

                Action = listView =>
                {
                    listView.HighlightNextItem(toggleSelection: true);
                },

                After = """
                      AAB 
                      AAC |
                      BBB |
                    > BCC |
                    """,
            },
            new Scenario("Highlight next item with selection should select item")
            {
                Before = """
                      AAB 
                      AAC |
                    > BBB |
                      BCC |
                    """,

                Action = listView =>
                {
                    listView.HighlightNextItem(toggleSelection: true);
                },

                After = """
                       AAB 
                       AAC |
                      *BBB |
                    >  BCC |
                    """,
            },
            new Scenario("Highlight next item with selection should unselect item")
            {
                Before = """
                      *AAB 
                      *AAC |
                    > *BBB |
                      *BCC |
                    """,

                Action = listView =>
                {
                    listView.HighlightNextItem(toggleSelection: true);
                },

                After = """
                      *AAB 
                      *AAC |
                       BBB |
                    > *BCC |
                    """,
            },
            new Scenario("Highlight item page down with selection should toggle all items on page except the new highlighted index")
            {
                Before = """
                    > *AAB | 
                       AAC |
                      *BBB |
                       BCC |
                    """,

                Action = listView =>
                {
                    listView.HighlightItemPageDown(toggleSelection: true);
                },

                After = """
                       AAB |
                      *AAC |
                       BBB |
                    >  BCC |
                    """,
            },
            new Scenario("Highlight first item with selection should toggle all items except the new highlighted index")
            {
                Before = """
                      *AAB
                       AAC |
                      *BBB |
                    >  BCC |
                       CCC
                    """,

                Action = listView =>
                {
                    listView.HighlightFirstItem(toggleSelection: true);
                },

                After = """
                    > *AAB |
                      *AAC |
                       BBB |
                      *BCC
                       CCC
                    """,
            },
            new Scenario("Highlight last item with selection should toggle all items except the new highlighted index")
            {
                Before = """
                      *AAB
                    >  AAC |
                      *BBB |
                       BCC |
                       CCC
                    """,

                Action = listView =>
                {
                    listView.HighlightLastItem(toggleSelection: true);
                },

                After = """
                      *AAB
                      *AAC
                       BBB |
                      *BCC |
                    >  CCC |
                    """,
            },
            new Scenario("Highlight first item with selection should toggle first item when it's already highlighted")
            {
                Before = """
                    > AAB |
                      AAC |
                      BBB |
                      BCC
                    """,

                Action = listView =>
                {
                    listView.HighlightFirstItem(toggleSelection: true);
                },

                After = """
                    > *AAB |
                       AAC |
                       BBB |
                       BCC
                    """,
            },
            new Scenario("Highlight last item with selection should toggle last item when it's already highlighted")
            {
                Before = """
                      AAB
                      AAC |
                      BBB |
                    > BCC |
                    """,

                Action = listView =>
                {
                    listView.HighlightLastItem(toggleSelection: true);
                },

                After = """
                       AAB
                       AAC |
                       BBB |
                    > *BCC |
                    """,
            },
        };
}
