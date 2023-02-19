using Xunit;

namespace InteractiveSelect.Tests;

partial class ListViewTests
{
    [Theory]
    [MemberData(nameof(FilteringScenarios))]
    public void FilteringTests(Scenario testCase)
        => testCase.Run();

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
