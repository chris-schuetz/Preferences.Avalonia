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
using Preferences.Common.Services;
using Preferences.Spectre.SampleApp.Pages;
using Preferences.Spectre.SampleApp.Rendering;
using SlimMessageBus;
using Spectre.Console;

namespace Preferences.Spectre.SampleApp;

public class InteractiveApplication : IConsumer<ShutdownCommand>
{
    private readonly HotKeyPage _hotKeyPage;
    private readonly ILogger<InteractiveApplication> _logger;
    private readonly PreferencesPage _preferencesPage;
    private readonly IMessageBus _bus;
    private bool _isRunning;

    public InteractiveApplication(
        ILogger<InteractiveApplication> logger,
        IMessageBus bus,
        PreferencesPage preferencesPage,
        HotKeyPage hotKeyPage)
    {
        _logger = logger;
        _bus = bus;
        _preferencesPage = preferencesPage;
        _hotKeyPage = hotKeyPage;
        _isRunning = false;
    }

    public async Task<int> RunAsync()
    {
        try
        {
            _logger.LogEntry();

            Initialize();
            await RunMainLoopAsync();

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application error occurred");
            return -1;
        }
        finally
        {
            AnsiConsole.Clear();
            _logger.LogExit();
        }
    }

    private void Initialize()
    {
        _logger.LogEntry();
        _isRunning = true;

        try
        {
            _logger.LogTrace("Attempting to get theme entry...");
            var themeEntry = _preferencesPage.GetEntry("Preferences.General", "Preferences.General.Theme");
            _logger.LogDebug("Theme entry: {ThemeEntry}", themeEntry?.Value ?? "null");

            if (themeEntry?.Value != null)
            {
                _logger.LogTrace("Updating theme to: {ThemeEntry}", themeEntry.Value);
                _screen.UpdateTheme(themeEntry.Value);
                _logger.LogDebug("Theme updated successfully");
            }
            else
            {
                _logger.LogDebug("No theme entry found, using default theme");
            }

            _logger.LogTrace("Initializing screen layout...");
            _screen.InitializeLayout();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing");
        }
        finally
        {
            _logger.LogExit();
        }
    }

    private async Task RunMainLoopAsync()
    {
        while (_isRunning)
        {
            _screen.RenderLayout();

            var keyInfo = Console.ReadKey(true);
            await _bus.Publish(message: new KeyInputMessage(keyInfo), cancellationToken: CancellationToken.None);
        }
    }

    public Task OnHandle(ShutdownCommand command, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Exit requested");
        _isRunning = false;
        return Task.CompletedTask;
    }
}
