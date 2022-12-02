using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace InteractiveSelect;

internal class PreviewPane
{
    private IReadOnlyList<ConsoleString> previewLines = Array.Empty<ConsoleString>();
    private PSObject? previewedObject;
    private readonly PSPropertyExpression? previewExpression;
    private int scrollOffset;
    private readonly int pageSize;

    public PreviewPane(PSPropertyExpression? previewExpression, int pageSize)
    {
        this.previewExpression = previewExpression;
        this.pageSize = pageSize;
    }

    public void SetPreviewedObject(PSObject? previewedObject)
    {
        if (ReferenceEquals(previewedObject, this.previewedObject))
            return;

        this.previewedObject = previewedObject;
        if (previewedObject != null)
            previewLines = GetPreviewLines(previewedObject).ToList();
        else
            previewLines = Array.Empty<ConsoleString>();

        scrollOffset = 0;
    }

    public bool HandleKey(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.DownArrow:
                scrollOffset = Math.Min(
                    scrollOffset + 1,
                    Math.Max(0, previewLines.Count - pageSize));
                return true;
            case ConsoleKey.UpArrow:
                scrollOffset = Math.Max(0, scrollOffset - 1);
                return true;
            case ConsoleKey.PageDown:
                scrollOffset = Math.Min(
                    scrollOffset + pageSize,
                    Math.Max(0, previewLines.Count - pageSize));
                return true;
            case ConsoleKey.PageUp:
                scrollOffset = Math.Max(0, scrollOffset - pageSize);
                return true;
        }

        return false;
    }

    public void Draw(Canvas canvas)
    {
        if (previewedObject != null)
        {
            int i = 0;
            int visibleLineCount = Math.Min(previewLines.Count, canvas.Height);

            for (; i < visibleLineCount; i++)
                canvas.FillLine(i, previewLines[i + scrollOffset]);

            for (; i < canvas.Height; i++)
                canvas.FillLine(i, ConsoleString.Empty);
        }
        else
        {
            canvas.Clear();
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
                            yield return ConsoleString.CreateStyled(splitLine);
                        }
                    }
                }
                
            }
        }
    }
}
