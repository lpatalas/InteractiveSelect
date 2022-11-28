using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using Microsoft.PowerShell.Commands;

namespace InteractiveSelect;

internal record MainLoopResult(IEnumerable<PSObject?> SelectedItems);

internal class MainWindow
{
    private readonly int height;
    private readonly ListPane listPane;
    private readonly PreviewPane previewPane;

    public MainWindow(
        IReadOnlyList<ListItem> listItems,
        int height,
        PSPropertyExpression? previewExpression)
    {
        this.height = height;

        listPane = new ListPane(listItems, height);
        previewPane = new PreviewPane(previewExpression);
    }

    public MainLoopResult RunMainLoop(PSHostUserInterface hostUI)
    {
        var initialCursorPosition = hostUI.RawUI.CursorPosition;

        previewPane.SetPreviewedObject(listPane.HighlightedObject);

        var selectedObjects = Enumerable.Empty<PSObject?>();
        var isExiting = false;
        while (!isExiting)
        {
            Draw(hostUI, initialCursorPosition);

            var pressedKey = Console.ReadKey(intercept: true);
            var keyHandled = listPane.HandleKey(pressedKey);
            if (keyHandled)
            {
                previewPane.SetPreviewedObject(listPane.HighlightedObject);
            }
            else
            {
                switch (pressedKey.Key)
                {
                    case ConsoleKey.Enter:
                        selectedObjects = listPane.GetSelectedObjects();
                        isExiting = true;
                        break;
                    case ConsoleKey.Escape:
                        isExiting = true;
                        break;
                }
            }
        }

        ClearConsole(hostUI, initialCursorPosition);

        return new MainLoopResult(selectedObjects);
    }

    private void Draw(PSHostUserInterface hostUI, Coordinates topLeft)
    {
        var mainArea = new Rectangle(
            topLeft.X,
            topLeft.Y,
            hostUI.RawUI.BufferSize.Width,
            topLeft.Y + height);

        var listPaneArea = new Rectangle(
            mainArea.Left,
            mainArea.Top,
            (mainArea.Left + mainArea.Right) / 2,
            mainArea.Bottom);

        var listPaneCanvas = new Canvas(hostUI, listPaneArea);
        listPane.Draw(listPaneCanvas);

        var previewPaneArea = new Rectangle(
            listPaneArea.Right,
            mainArea.Top,
            mainArea.Right,
            mainArea.Bottom);

        var previewPaneCanvas = new Canvas(hostUI, previewPaneArea);
        previewPane.Draw(previewPaneCanvas);
    }

    private void ClearConsole(PSHostUserInterface hostUI, Coordinates topLeft)
    {
        var area = new Rectangle(
            0,
            topLeft.Y,
            hostUI.RawUI.BufferSize.Width,
            topLeft.Y + height);

        var canvas = new Canvas(hostUI, area);
        canvas.Clear();
    }
}
