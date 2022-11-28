using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace InteractiveSelect;

internal class PreviewPane
{
    private PSObject? previewedObject;
    private readonly PSPropertyExpression? propertyExpression;

    public PreviewPane(PSPropertyExpression? propertyExpression)
    {
        this.propertyExpression = propertyExpression;
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
        if (propertyExpression == null)
            yield break;

        List<PSPropertyExpressionResult> results = propertyExpression.GetValues(obj);
        if (results.Count > 0)
        {
            foreach (var result in results)
            {
                var line = result?.Result?.ToString() ?? string.Empty;
                var splitLines = line.Split(Environment.NewLine);
                foreach (var splitLine in splitLines)
                {
                    yield return splitLine.RemoveControlCharacters();
                }
            }
        }
    }
}
