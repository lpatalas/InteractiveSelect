﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace InteractiveSelect;

internal class PreviewPane
{
    private PSObject? previewedObject;
    private readonly PSPropertyExpression? previewExpression;
    private readonly ScrollView<ConsoleString> scrollView;

    public PreviewPane(PSPropertyExpression? previewExpression, int height)
    {
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
        canvas.DrawHeader(isActive, ConsoleString.CreatePlainText("Preview"));
        var scrollViewCanvas = canvas.GetSubArea(0, 1, canvas.Width, canvas.Height - 1);
        DrawScrollView(scrollViewCanvas);
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
                            yield return ConsoleString.CreateStyled(splitLine);
                        }
                    }
                }
                
            }
        }
    }
}
