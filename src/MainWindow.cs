using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace InteractiveSelect;

internal record MainLoopResult(IEnumerable<object?> SelectedItems);

internal class MainWindow
{
    private enum ActivePane
    {
        List,
        Preview
    }

    private const int separatorWidth = 1;

    private ActivePane activePane = ActivePane.List;
    private readonly KeyBindings keyBindings;
    private readonly int height;
    private readonly ListPane listPane;
    private readonly PreviewPane? previewPane;

    public MainWindow(
        PSHostUserInterface hostUI,
        KeyBindings keyBindings,
        IReadOnlyList<InputObject> inputObjects,
        int height,
        PSPropertyExpression? previewExpression)
    {
        this.keyBindings = keyBindings;
        this.height = height;

        int maxListPaneWidth = previewExpression switch
        {
            null => hostUI.RawUI.WindowSize.Width,
            _ => hostUI.RawUI.WindowSize.Width / 2
        };

        listPane = new ListPane(
            inputObjects,
            maxListPaneWidth,
            height,
            OnHighlightedListItemChanged);

        if (previewExpression != null)
        {
            int previewWidth = hostUI.RawUI.WindowSize.Width - listPane.Width - separatorWidth;
            previewPane = new PreviewPane(previewExpression, previewWidth, height);
        }
    }

    public MainLoopResult RunMainLoop(PSHostUserInterface hostUI)
    {
        var initialCursorPosition = ReserveBufferSpace(hostUI);

        listPane.Initialize();

        var selectedObjects = Enumerable.Empty<object?>();
        var isExiting = false;
        while (!isExiting)
        {
            Draw(hostUI, initialCursorPosition);

            var pressedKey = Console.ReadKey(intercept: true);

            bool keyHandled = false;

            var scriptApi = new ExternalScriptApi(listPane.HighlightedValue);
            if (keyBindings.HandleKey(pressedKey, scriptApi))
            {
                keyHandled = true;
                if (scriptApi.WasExitRequested)
                {
                    if (scriptApi.Result is IEnumerable<object?> collection)
                        selectedObjects = collection;
                    else if (scriptApi.Result is { } value)
                        selectedObjects = new[] { value };

                    isExiting = true;
                }
            }

            if (!keyHandled)
                keyHandled = HandleKey(pressedKey);

            if (!keyHandled)
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

    private bool HandleKey(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Key == ConsoleKey.Tab && previewPane != null)
        {
            if (activePane == ActivePane.List)
                activePane = ActivePane.Preview;
            else
                activePane = ActivePane.List;

            return true;
        }

        if (activePane == ActivePane.List)
            return listPane.HandleKey(keyInfo);
        else
            return previewPane?.HandleKey(keyInfo) ?? false;
    }

    private void OnHighlightedListItemChanged(PSObject? selectedItem)
    {
        previewPane?.SetPreviewedObject(selectedItem);
    }

    private Coordinates ReserveBufferSpace(PSHostUserInterface hostUI)
    {
        var initCommand = new string('\n', height) + EscapeSequence.CursorUp(height);
        hostUI.Write(initCommand);
        return hostUI.RawUI.CursorPosition;
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
            mainArea.Left + listPane.Width,
            mainArea.Bottom);

        var listPaneCanvas = new Canvas(hostUI, listPaneArea);
        listPane.Draw(listPaneCanvas, activePane == ActivePane.List);

        if (previewPane != null)
        {
            var previewPaneArea = new Rectangle(
                listPaneArea.Right + 1,
                mainArea.Top,
                mainArea.Right,
                mainArea.Bottom);

            var previewPaneCanvas = new Canvas(hostUI, previewPaneArea);
            previewPane?.Draw(previewPaneCanvas, activePane == ActivePane.Preview);

            var separatorArea = new Rectangle(
                listPaneArea.Right,
                mainArea.Top,
                previewPaneArea.Left,
                mainArea.Bottom);

            var separatorText = ConsoleString.CreateStyled(Theme.Instance.Border + "\u2502");
            var separatorCanvas = new Canvas(hostUI, separatorArea);
            separatorCanvas.FillLine(0, ConsoleString.CreateStyled(Theme.Instance.Border + "\u252c"));
            for (int i = 1; i < separatorArea.GetHeight(); i++)
                separatorCanvas.FillLine(i, separatorText);
        }
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
