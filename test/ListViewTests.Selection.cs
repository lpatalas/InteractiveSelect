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
                    """,

                ExpectNoChangeNotifications = true
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
                    """,

                ExpectNoChangeNotifications = true
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
                    """,

                ExpectNoChangeNotifications = true
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
                    """,

                ExpectNoChangeNotifications = true
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
                    """,

                ExpectNoChangeNotifications = true
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
                    """,

                ExpectNoChangeNotifications = true
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
                    """,

                ExpectNoChangeNotifications = true
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
                    """,

                ExpectNoChangeNotifications = true
            },
        };
}
