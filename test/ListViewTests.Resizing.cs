using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace InteractiveSelect.Tests;

partial class ListViewTests
{
    [Theory]
    [MemberData(nameof(ResizingScenarios))]
    public void RunResizingScenarios(Scenario scenario)
        => scenario.Run();

    public static TheoryData<Scenario> ResizingScenarios =>
        new()
        {
            new Scenario("Keep highlighted index if it's still visible on resized page")
            {
                Before = """
                    > AAB |
                      AAC |
                      BBB |
                      BCC
                    """,

                Action = listView =>
                    listView.SetPageSize(2),

                After = """
                    > AAB |
                      AAC |
                      BBB
                      BCC
                    """,

                ExpectNoChangeNotifications = true
            },
            new Scenario("Keep highlighted index if it's still visible on resized page (at the end)")
            {
                Before = """
                      AAB |
                    > AAC |
                      BBB |
                      BCC
                    """,

                Action = listView =>
                    listView.SetPageSize(2),

                After = """
                      AAB |
                    > AAC |
                      BBB
                      BCC
                    """,

                ExpectNoChangeNotifications = true
            },
            new Scenario("Do not scroll if highlighted index is still visible on shrunk page")
            {
                Before = """
                      AAB
                    > AAC |
                      BBB |
                      BCC |
                    """,

                Action = listView =>
                    listView.SetPageSize(2),

                After = """
                      AAB
                    > AAC |
                      BBB |
                      BCC
                    """,

                ExpectNoChangeNotifications = true
            },
            new Scenario("Do not scroll if highlighted index is still visible on shrunk page (at the end)")
            {
                Before = """
                      AAB
                      AAC |
                    > BBB |
                      BCC |
                    """,

                Action = listView =>
                    listView.SetPageSize(2),

                After = """
                      AAB
                      AAC |
                    > BBB |
                      BCC
                    """,

                ExpectNoChangeNotifications = true
            },
            new Scenario("Scroll if highlighted index is outside shrunk page")
            {
                Before = """
                      AAB |
                      AAC |
                    > BBB |
                      BCC
                    """,

                Action = listView =>
                    listView.SetPageSize(2),

                After = """
                      AAB
                      AAC |
                    > BBB |
                      BCC
                    """,

                ExpectNoChangeNotifications = true
            },
            new Scenario("Scroll if last item is outside shrunk page")
            {
                Before = """
                      AAB
                      AAC |
                      BBB |
                    > BCC |
                    """,

                Action = listView =>
                    listView.SetPageSize(2),

                After = """
                      AAB
                      AAC
                      BBB |
                    > BCC |
                    """,

                ExpectNoChangeNotifications = true
            },
        };
}
