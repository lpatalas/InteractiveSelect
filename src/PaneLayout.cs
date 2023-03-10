using System;
using System.Management.Automation.Host;

namespace InteractiveSelect;

internal enum SplitDirection
{
    Horizontal,
    Vertical
}

internal interface IPaneLayout
{
    ListPane ListPane { get; }
    PreviewPane? PreviewPane { get; }

    bool ToggleActivePane();
    bool HandleKey(ConsoleKeyInfo keyInfo);
    void Resize(int width, int height);
    void Draw(PSHostUserInterface hostUI, Coordinates topLeft);
}

internal class SinglePaneLayout : IPaneLayout
{
    private readonly ListPane listPane;

    public ListPane ListPane => listPane;
    public PreviewPane? PreviewPane => null;

    public SinglePaneLayout(ListPane listPane)
    {
        this.listPane = listPane;
    }

    public bool ToggleActivePane()
        => false;

    public bool HandleKey(ConsoleKeyInfo keyInfo)
        => listPane.HandleKey(keyInfo);

    public void Resize(int width, int height)
    {
        var maxListSize = listPane.GetMaximumSize();
        var listPaneSize = new Size(
            Math.Min(width, maxListSize.Width),
            height);
        listPane.Resize(listPaneSize.Width, listPaneSize.Height);
    }

    public void Draw(PSHostUserInterface hostUI, Coordinates topLeft)
    {
        var canvasArea = new Rect(topLeft.X, topLeft.Y, listPane.Width, listPane.Height);
        var canvas = new Canvas(hostUI, canvasArea);
        listPane.Draw(canvas, true);
    }
}

internal class SplitPaneLayout : IPaneLayout
{
    private const int minimumPaneSize = 2;
    private const int separatorSize = 1;

    private enum ActivePane
    {
        List,
        Preview
    }

    private ActivePane activePane = ActivePane.List;
    private Size currentSize;
    private readonly ListPane listPane;
    private readonly PreviewPane previewPane;
    private readonly IDimensionValue? splitOffset;
    private readonly SplitDirection splitDirection;

    public ListPane ListPane => listPane;
    public PreviewPane? PreviewPane => previewPane;

    public SplitPaneLayout(
        ListPane listPane,
        PreviewPane previewPane,
        IDimensionValue? splitOffset,
        SplitDirection splitDirection)
    {
        this.listPane = listPane;
        this.previewPane = previewPane;
        this.splitOffset = splitOffset;
        this.splitDirection = splitDirection;
    }

    public bool ToggleActivePane()
    {
        if (activePane == ActivePane.List)
            activePane = ActivePane.Preview;
        else
            activePane = ActivePane.List;

        return true;
    }

    public bool HandleKey(ConsoleKeyInfo keyInfo)
    {
        if (activePane == ActivePane.List)
            return listPane.HandleKey(keyInfo);
        else
            return previewPane.HandleKey(keyInfo);
    }

    public void Resize(int width, int height)
    {
        currentSize = new Size(width, height);

        Size listPaneSize = ComputeListPaneSize(width, height);
        Size previewPaneSize = ComputePreviewPaneSize(width, height, listPaneSize);

        listPane.Resize(listPaneSize.Width, listPaneSize.Height);
        previewPane.Resize(previewPaneSize.Width, previewPaneSize.Height);
    }

    private Size ComputeListPaneSize(int totalWidth, int totalHeight)
    {
        var maxListSize = listPane.GetMaximumSize();

        if (splitDirection == SplitDirection.Horizontal)
        {
            if (splitOffset is IDimensionValue offset)
            {
                var paneWidth = offset.CalculateAbsoluteValue(totalWidth);
                if (paneWidth < minimumPaneSize)
                    paneWidth = minimumPaneSize;
                else if (paneWidth + separatorSize + minimumPaneSize > totalWidth)
                    paneWidth = totalWidth - separatorSize - minimumPaneSize;

                return new Size(paneWidth, totalHeight);
            }
            else
            {
                return new Size(
                    Math.Min(maxListSize.Width, totalWidth / 2),
                    totalHeight);
            }
        }
        else if (splitDirection == SplitDirection.Vertical)
        {
            if (splitOffset is IDimensionValue offset)
            {
                var paneHeight = offset.CalculateAbsoluteValue(totalHeight);
                if (paneHeight < minimumPaneSize)
                    paneHeight = minimumPaneSize;
                else if (paneHeight + minimumPaneSize > totalHeight)
                    paneHeight = totalHeight - minimumPaneSize;

                return new Size(totalWidth, paneHeight);
            }
            else
            {
                return new Size(
                    totalWidth,
                    Math.Min(maxListSize.Height, totalHeight / 2));
            }
        }
        else
        {
            throw new InvalidOperationException($"Invalid enum value {splitDirection}");
        }
    }

    private Size ComputePreviewPaneSize(int totalWidth, int totalHeight, Size listPaneSize)
    {
        if (splitDirection == SplitDirection.Horizontal)
        {
            return new Size(
                totalWidth - listPaneSize.Width - separatorSize,
                totalHeight);
        }
        else if (splitDirection == SplitDirection.Vertical)
        {
            return new Size(
                totalWidth,
                totalHeight - listPaneSize.Height);
        }
        else
        {
            throw new InvalidOperationException($"Invalid enum value {splitDirection}");
        }
    }

    public void Draw(PSHostUserInterface hostUI, Coordinates topLeft)
    {
        var listPaneArea = new Rect(topLeft.X, topLeft.Y, listPane.Width, listPane.Height);
        var listPaneCanvas = new Canvas(hostUI, listPaneArea);
        listPane.Draw(listPaneCanvas, activePane == ActivePane.List);

        var previewPaneArea = GetPreviewPaneArea(topLeft);
        var previewPaneCanvas = new Canvas(hostUI, previewPaneArea);
        previewPane.Draw(previewPaneCanvas, activePane == ActivePane.Preview);

        if (splitDirection == SplitDirection.Horizontal)
            DrawSeparator(hostUI, topLeft);
    }

    private Rect GetPreviewPaneArea(Coordinates topLeft)
    {
        switch (splitDirection)
        {
            case SplitDirection.Horizontal:
                return new Rect(
                    topLeft.X + listPane.Width + separatorSize,
                    topLeft.Y,
                    previewPane.Width,
                    previewPane.Height);
            case SplitDirection.Vertical:
                return new Rect(
                    topLeft.X,
                    topLeft.Y + listPane.Height,
                    previewPane.Width,
                    previewPane.Height);
            default:
                throw new InvalidOperationException($"Invalid enum value {splitDirection}");
        }
    }

    private void DrawSeparator(PSHostUserInterface hostUI, Coordinates topLeft)
    {
        var separatorArea = new Rect(
            topLeft.X + listPane.Width,
            topLeft.Y,
            1,
            currentSize.Height);

        var separatorCanvas = new Canvas(hostUI, separatorArea);
        separatorCanvas.FillLine(0, ConsoleString.CreateStyled(Theme.Instance.Border + "\u252c"));

        var separatorText = ConsoleString.CreateStyled(Theme.Instance.Border + "\u2502");
        for (int i = 1; i < separatorArea.Height; i++)
            separatorCanvas.FillLine(i, separatorText);
    }
}
