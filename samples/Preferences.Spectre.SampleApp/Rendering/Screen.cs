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
using Preferences.Common.Messages;
using Preferences.Common.SampleApp;
using Preferences.Spectre.SampleApp.Pages;
using Spectre.Console.Rendering;

namespace Preferences.Spectre.SampleApp.Rendering;

public class Screen : IScreen
{
    private readonly ScreenLayout _screenLayout;
    private readonly ILogger<Screen> _logger;
    private IRenderable? _currentPageContent;

    public Screen(
        ILogger<Screen> logger,
        ScreenLayout screenLayout)
    {
        _logger = logger;
        _screenLayout = screenLayout;
    }

    public IPage? CurrentPage { get; set; }

    public string StatusBarStatusText { get; set; } = string.Empty;

    public string StatusBarHintText { get; set; } = string.Empty;

    public StatusMessageType StatusBarStatusMessageType { get; set; }

    public string Title { get; set; } = string.Empty;

    public void UpdateTheme(string themeName)
    {
        _screenLayout.UpdateTheme(themeName);
    }

    public void InitializeLayout()
    {
        _logger.LogEntry();
        _screenLayout.ClearScreen();

        Console.CursorVisible = false;
        Console.TreatControlCAsInput = true;

        _logger.LogExit();
    }

    public void RenderLayout()
    {
        _logger.LogEntry();

        try
        {
            _currentPageContent = CurrentPage?.Draw(_screenLayout, this);
            _screenLayout.ClearScreen();
            RenderTitle();
            RenderMainContent();
            RenderStatusBar();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error rendering layout");
            // TODO write the exception to the AnsiConsole and wait until user input
        }

        _logger.LogExit();
    }

    private void RenderTitle()
    {
        _screenLayout.RenderMainHeader(Title);
    }

    private void RenderMainContent()
    {
        _screenLayout.ClearMainContent();
        _screenLayout.RenderMainContent(_currentPageContent);
    }

    private void RenderStatusBar()
    {
        // TODO shorten status if necessary
        _screenLayout.RenderStatusBar(StatusBarHintText, StatusBarStatusText, StatusBarStatusMessageType);
    }
}
