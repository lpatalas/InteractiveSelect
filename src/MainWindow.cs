using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace InteractiveSelect;

internal record MainLoopResult(IEnumerable<object?> SelectedItems);

internal class MainWindow
{
    private const int separatorWidth = 1;

    private readonly KeyBindings keyBindings;
    private readonly int height;
    private readonly IPaneLayout paneLayout;

    public MainWindow(
        PSHostUserInterface hostUI,
        KeyBindings keyBindings,
        IReadOnlyList<InputObject> inputObjects,
        int initialWidth,
        int initialHeight,
        IDimensionValue? splitOffset,
        SplitDirection splitDirection,
        PSPropertyExpression? previewExpression)
    {
        this.keyBindings = keyBindings;
        this.height = initialHeight;

        int maxListPaneWidth = previewExpression switch
        {
            null => hostUI.RawUI.WindowSize.Width,
            _ => hostUI.RawUI.WindowSize.Width / 2
        };

        var listPane = new ListPane(
            inputObjects,
            maxListPaneWidth,
            height,
            OnHighlightedListItemChanged);

        if (previewExpression != null)
        {
            int previewWidth = hostUI.RawUI.WindowSize.Width - listPane.Width - separatorWidth;
            var previewPane = new PreviewPane(previewExpression, previewWidth, height);
            paneLayout = new SplitPaneLayout(
                listPane,
                previewPane,
                splitOffset,
                splitDirection);
        }
        else
        {
            paneLayout = new SinglePaneLayout(listPane);
        }

        paneLayout.Resize(initialWidth, initialHeight);
    }

    public MainLoopResult RunMainLoop(PSHostUserInterface hostUI)
    {
        var initialCursorPosition = ReserveBufferSpace(hostUI);

        paneLayout.PreviewPane?.SetPreviewedObject(paneLayout.ListPane.HighlightedValue);

        var selectedObjects = Enumerable.Empty<object?>();
        var isExiting = false;
        while (!isExiting)
        {
            paneLayout.Draw(hostUI, initialCursorPosition);

            var pressedKey = Console.ReadKey(intercept: true);

            bool keyHandled = false;

            var scriptApi = new ExternalScriptApi(paneLayout.ListPane.HighlightedValue);
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
                        selectedObjects = paneLayout.ListPane.GetSelectedObjects();
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
        if (keyInfo.Key == ConsoleKey.Tab)
        {
            return paneLayout.ToggleActivePane();
        }

        return paneLayout.HandleKey(keyInfo);
    }

    private void OnHighlightedListItemChanged(PSObject? selectedItem)
    {
        paneLayout.PreviewPane?.SetPreviewedObject(selectedItem);
    }

    private Coordinates ReserveBufferSpace(PSHostUserInterface hostUI)
    {
        var initCommand = new string('\n', height) + EscapeSequence.CursorUp(height);
        hostUI.Write(initCommand);
        return hostUI.RawUI.CursorPosition;
    }

    private void ClearConsole(PSHostUserInterface hostUI, Coordinates topLeft)
    {
        var area = new Rect(
            0,
            topLeft.Y,
            hostUI.RawUI.BufferSize.Width,
            height);

        var canvas = new Canvas(hostUI, area);
        canvas.Clear();
    }
}
