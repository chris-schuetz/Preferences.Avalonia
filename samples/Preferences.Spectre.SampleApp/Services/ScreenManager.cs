// Copyright (c) 2025 Christopher Schuetz
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Microsoft.Extensions.Logging;
using Preferences.Common;
using Spectre.Console;

namespace Preferences.Spectre.SampleApp.Services;

public class ScreenManager
{
    private readonly ILogger<ScreenManager> _logger;
    private readonly ConsoleRenderer _renderer;

    private readonly PreferencesManagerService _preferencesManager;

    public ScreenManager(
        ILogger<ScreenManager> logger,
        ConsoleRenderer renderer,
        PreferencesManagerService preferencesManager)
    {
        _logger = logger;
        _renderer = renderer;
        _preferencesManager = preferencesManager;
    }

    public void InitializeLayout()
    {
        _renderer.ClearScreen();

        // Set up console
        Console.CursorVisible = false;
        Console.TreatControlCAsInput = true;

        _logger.LogDebug("Screen layout initialized");
    }

    public void RenderLayout()
    {
        // Clear screen and position cursor at top
        _renderer.ClearScreen();

        // Render application title
        RenderTitle();

        // Render separator
        _renderer.RenderThinSeparator();

        // Render main content area (empty for now)
        RenderMainContent();

        // Render status bar at bottom
        RenderStatusBar();
    }

    public void RefreshLayout()
    {
        _renderer.ClearScreen();
        RenderLayout();
    }

    private void RenderTitle()
    {
        var title = "Preferences Sample Application";
        _renderer.RenderMainHeader(title);
        _renderer.AddVerticalSpace(1);
    }

    private void RenderMainContent()
    {
        var contentHeight = GetMainContentHeight();

        // Empty main content area - fill with blank lines
        for (int i = 0; i < contentHeight; i++)
        {
            AnsiConsole.WriteLine();
        }
    }

    private void RenderStatusBar()
    {
        var statusMessage = "Sample Application for Preferences Library";
        var shortcuts = "Ready";

        // Position at bottom of screen
        try
        {
            Console.SetCursorPosition(0, Console.WindowHeight - 2);
        }
        catch
        {
            // Fallback if we can't position cursor
        }

        _renderer.RenderStatusBar(statusMessage, shortcuts);
    }

    private int GetMainContentHeight()
    {
        try
        {
            // Calculate available height for main content
            var totalHeight = Console.WindowHeight;
            var usedHeight = 8; // Title + menu + separator + status bar + margins
            return Math.Max(5, totalHeight - usedHeight);
        }
        catch
        {
            return 10; // Fallback height
        }
    }

    public void ShowTemporaryMessage(string message, MessageType type = MessageType.Info)
    {
        var oldPosition = Console.GetCursorPosition();

        try
        {
            // Show message in main content area
            Console.SetCursorPosition(2, 10);

            switch (type)
            {
                case MessageType.Success:
                    _renderer.RenderSuccessMessage(message);
                    break;
                case MessageType.Warning:
                    _renderer.RenderWarningMessage(message);
                    break;
                case MessageType.Error:
                    _renderer.RenderErrorMessage(message);
                    break;
                default:
                    _renderer.RenderInfoMessage(message);
                    break;
            }

            // Wait for user acknowledgment
            _renderer.PauseForUser();

            // Refresh layout to clear message
            RefreshLayout();
        }
        catch
        {
            // Fallback: restore cursor position
            try
            {
                Console.SetCursorPosition(oldPosition.Left, oldPosition.Top);
            }
            catch { }
        }
    }
}

public enum MessageType
{
    Info,
    Success,
    Warning,
    Error
}
