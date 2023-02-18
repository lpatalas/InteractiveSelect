using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace InteractiveSelect;

internal class PreviewPane
{
    private readonly int maxLineLength;
    private PSObject? previewedObject;
    private readonly PSPropertyExpression? previewExpression;
    private readonly ScrollView<ConsoleString> scrollView;

    public PreviewPane(PSPropertyExpression? previewExpression, int width, int height)
    {
        this.maxLineLength = width - 1; // "- 1" to make space for the scrollbar
        this.previewExpression = previewExpression;
        this.scrollView = new ScrollView<ConsoleString>(pageSize: height - 1);
    }

    public void SetPreviewedObject(PSObject? previewedObject)
    {
        if (ReferenceEquals(previewedObject, this.previewedObject))
            return;

        this.previewedObject = previewedObject;

        IReadOnlyList<ConsoleString> previewLines = previewedObject != null
            ? GetPreviewLines(previewedObject).ToList()
            : Array.Empty<ConsoleString>();

        scrollView.SetItems(previewLines);
    }

    public bool HandleKey(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.DownArrow:
                scrollView.ScrollDown();
                return true;
            case ConsoleKey.UpArrow:
                scrollView.ScrollUp();
                return true;
            case ConsoleKey.PageDown:
                scrollView.ScrollPageDown();
                return true;
            case ConsoleKey.PageUp:
                scrollView.ScrollPageUp();
                return true;
            case ConsoleKey.Home:
                scrollView.ScrollToTheTop();
                return true;
            case ConsoleKey.End:
                scrollView.ScrollToTheBottom();
                return true;
        }

        return false;
    }

    public void Draw(Canvas canvas, bool isActive)
    {
        var currentPage = scrollView.GetCurrentPage();
        var headerText = $"{currentPage.FirstIndex + 1}-{currentPage.LastIndex + 1}/{scrollView.TotalCount}";

        canvas.DrawHeader(isActive, ConsoleString.CreatePlainText(headerText));

        var scrollViewCanvas = canvas.GetSubArea(0, 1, canvas.Width - 1, canvas.Height - 1);
        DrawScrollView(scrollViewCanvas);

        var scrollBarCanvas = canvas.GetSubArea(canvas.Width - 1, 1, 1, canvas.Height - 1);
        DrawScrollBar(scrollBarCanvas);
    }

    private void DrawScrollView(Canvas canvas)
    {
        var page = scrollView.GetCurrentPage();
        int lineIndex = 0;

        for (; lineIndex < page.Count; lineIndex++)
            canvas.FillLine(lineIndex, page[lineIndex]);

        for (; lineIndex < canvas.Height; lineIndex++)
            canvas.FillLine(lineIndex, ConsoleString.Empty);
    }

    private void DrawScrollBar(Canvas canvas)
    {
        var page = scrollView.GetCurrentPage();
        var scrollBar = ScrollBarLayout.Compute(
            canvas.Height,
            page.FirstIndex,
            page.Count,
            scrollView.TotalCount);

        for (int i = 0; i < scrollBar.TotalSize; i++)
        {
            var glyph = scrollBar.GetVerticalGlyph(i);
            canvas.FillLine(i, ConsoleString.CreatePlainText(glyph.ToString()));
        }
    }

    private IEnumerable<ConsoleString> GetPreviewLines(PSObject obj)
    {
        if (previewExpression == null)
            yield break;

        List<PSPropertyExpressionResult> results = previewExpression.GetValues(obj);
        if (results.Count > 0)
        {
            foreach (var result in results)
            {
                var psObjectResult = (result.Result is not null)
                    ? PSObject.AsPSObject(result.Result)
                    : null;

                IEnumerable<string?> subResults;

                if (psObjectResult?.BaseObject is IEnumerable<object> collection)
                    subResults = collection.Select(x => x?.ToString());
                else
                    subResults = Enumerable.Repeat(result.Result?.ToString(), 1);

                foreach (var subResult in subResults)
                {
                    if (subResult is null)
                    {
                        yield return ConsoleString.Empty;
                    }
                    else
                    {
                        var splitLines = subResult.Split('\n');
                        foreach (var splitLine in splitLines)
                        {
                            var line = ConsoleString.CreateStyled(splitLine);
                            var wrappedLines = line.WordWrap(maxLineLength);
                            foreach (var item in wrappedLines)
                            {
                                yield return item;
                            }
                        }
                    }
                }
                
            }
        }
    }
}
