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
using Microsoft.Extensions.Options;
using Preferences.Common;
using Preferences.Common.Messages;
using Preferences.Common.SampleApp;
using Preferences.Spectre.SampleApp.Pages;
using SlimMessageBus;
using Spectre.Console.Rendering;

namespace Preferences.Spectre.SampleApp.Rendering;

public class Screen : IScreen, IConsumer<StatusMessage>, IConsumer<ShowHotKeysCommand>,
    IConsumer<OpenPreferencesCommand>
{
    private readonly HotKeyPage _hotKeyPage;
    private readonly ILogger<Screen> _logger;
    private readonly IOptionsMonitor<PreferencesOptions> _options;
    private readonly IDisposable? _optionsSubscription;
    private readonly PreferencesPage _preferencesPage;
    private readonly ScreenLayout _screenLayout;
    private IRenderable? _currentPageContent;

    public Screen(
        ILogger<Screen> logger,
        IOptionsMonitor<PreferencesOptions> options,
        ScreenLayout screenLayout,
        HotKeyPage hotKeyPage,
        PreferencesPage preferencesPage)
    {
        _logger = logger;
        _options = options;
        _screenLayout = screenLayout;
        _hotKeyPage = hotKeyPage;
        _preferencesPage = preferencesPage;

        _optionsSubscription = _options.OnChange(UpdateTheme);
        UpdateTheme(_options.CurrentValue, null);
    }

    public IPage? CurrentPage { get; set; }

    public Task OnHandle(OpenPreferencesCommand message, CancellationToken cancellationToken)
    {
        CurrentPage = _preferencesPage;
        return Task.CompletedTask;
    }

    public Task OnHandle(ShowHotKeysCommand message, CancellationToken cancellationToken)
    {
        CurrentPage = _hotKeyPage;
        return Task.CompletedTask;
    }

    public Task OnHandle(StatusMessage message, CancellationToken cancellationToken)
    {
        StatusBarStatusText = message.Text;
        StatusBarStatusMessageType = message.Type;
        return Task.CompletedTask;
    }

    public string StatusBarStatusText { get; set; } = string.Empty;

    public string StatusBarHintText { get; set; } = string.Empty;

    public StatusMessageType StatusBarStatusMessageType { get; set; }

    public string Title { get; set; } = string.Empty;

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
            _currentPageContent = CurrentPage?.Draw(_screenLayout);
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

    private void UpdateTheme(PreferencesOptions options, string? arg2)
    {
        var themeName = options.Sections.First(section =>
            section.Name.Equals("Preferences.General", StringComparison.Ordinal)).Entries.First(entry =>
            entry.Name.Equals("Preferences.General.Theme", StringComparison.Ordinal)).Value;
        _screenLayout.UpdateTheme(themeName);
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
