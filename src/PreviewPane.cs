using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace InteractiveSelect;

internal class PreviewPane
{
    private PSObject? previewedObject;
    private readonly PSPropertyExpression? previewExpression;

    public PreviewPane(PSPropertyExpression? previewExpression)
    {
        this.previewExpression = previewExpression;
    }

    public void SetPreviewedObject(PSObject? previewedObject)
    {
        this.previewedObject = previewedObject;
    }

    public bool HandleKey(ConsoleKeyInfo keyInfo)
    {
        return false;
    }

    public void Draw(Canvas canvas)
    {
        if (previewedObject != null)
        {
            var previewLines = GetPreviewLines(previewedObject);
            var lineIndex = 0;
            foreach (var item in previewLines.Take(canvas.Height))
            {
                canvas.FillLine(lineIndex, item);
                lineIndex++;
                if (lineIndex == canvas.Height)
                    break;
            }

            for (; lineIndex < canvas.Height; lineIndex++)
            {
                canvas.FillLine(lineIndex, string.Empty);
            }
        }
        else
        {
            canvas.Clear();
        }
    }

    private IEnumerable<string> GetPreviewLines(PSObject obj)
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
                        yield return string.Empty;
                    }
                    else
                    {
                        var splitLines = subResult.Split('\n');
                        foreach (var splitLine in splitLines)
                        {
                            yield return splitLine.RemoveControlCharactersExceptEsc();
                        }
                    }
                }
                
            }
        }
    }
}
