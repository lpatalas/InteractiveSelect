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

    public PreviewPane(PSPropertyExpression? previewExpression)
    {
        this.previewExpression = previewExpression;
    }

    public void SetPreviewedObject(PSObject? previewedObject)
    {
        this.previewedObject = previewedObject;
        if (previewedObject != null)
            previewLines = GetPreviewLines(previewedObject).ToList();
        else
            previewLines = Array.Empty<ConsoleString>();

        scrollOffset = 0;
    }

    public bool HandleKey(ConsoleKeyInfo keyInfo)
    {
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
