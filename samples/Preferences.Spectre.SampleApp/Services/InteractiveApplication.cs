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
using Spectre.Console;

namespace Preferences.Spectre.SampleApp.Services;

public class InteractiveApplication
{
    private readonly ILogger<InteractiveApplication> _logger;
    private readonly ConsoleRenderer _renderer;
    private readonly ScreenManager _screenManager;
    private readonly PreferencesManagerService _preferencesManager;
    private readonly HotKeyService _hotKeyService;
    private bool _isRunning;

    public InteractiveApplication(
        ILogger<InteractiveApplication> logger,
        ConsoleRenderer renderer,
        ScreenManager screenManager,
        PreferencesManagerService preferencesManager,
        HotKeyService hotKeyService)
    {
        _logger = logger;
        _renderer = renderer;
        _screenManager = screenManager;
        _preferencesManager = preferencesManager;
        _hotKeyService = hotKeyService;
        _isRunning = false;
    }

    public async Task<int> RunAsync()
    {
        try
        {
            _logger.LogInformation("Starting Preferences.Spectre.SampleApp");

            Initialize();
            await RunMainLoopAsync();

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application error occurred");
            _renderer.RenderErrorMessage($"Application error: {ex.Message}");
            return -1;
        }
        finally
        {
            Cleanup();
        }
    }

    private void Initialize()
    {
        Console.WriteLine("InteractiveApplication.Initialize() called");
        _isRunning = true;

        try
        {
            Console.WriteLine("Attempting to get theme entry...");
            // Apply initial theme from preferences
            var themeEntry = _preferencesManager.GetEntry("Preferences.General", "Preferences.General.Theme");
            Console.WriteLine($"Theme entry: {themeEntry?.Value ?? "null"}");

            if (themeEntry?.Value != null)
            {
                Console.WriteLine($"Updating theme to: {themeEntry.Value}");
                _renderer.UpdateTheme(themeEntry.Value);
                Console.WriteLine("Theme updated successfully");
            }
            else
            {
                Console.WriteLine("No theme entry found, using default theme");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting theme entry: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Continue with default theme
        }

        Console.WriteLine("Clearing screen...");
        _renderer.ClearScreen();

        Console.WriteLine("Initializing screen layout...");
        _screenManager.InitializeLayout();

        Console.WriteLine("Wiring up menu actions...");

        _logger.LogDebug("Application initialized");
        Console.WriteLine("InteractiveApplication.Initialize() completed successfully");
    }

    private async Task RunMainLoopAsync()
    {
        while (_isRunning)
        {
            _screenManager.RenderLayout();

            var keyInfo = Console.ReadKey(true);
            await HandleKeyInputAsync(keyInfo);
        }
    }

    private async Task HandleKeyInputAsync(ConsoleKeyInfo keyInfo)
    {
        try
        {
            // Check for global hotkeys first
            if (await HandleGlobalHotKeysAsync(keyInfo))
                return;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling key input");
            _renderer.RenderErrorMessage($"Input error: {ex.Message}");
        }
    }

    private async Task<bool> HandleGlobalHotKeysAsync(ConsoleKeyInfo keyInfo)
    {
        // Get all preferences from PreferencesOptions
        var preferences = _preferencesManager.GetCurrentPreferences();
        if (preferences?.Sections == null)
        {
            return false;
        }

        // Build dictionary of all hotkey entries and their actions
        var hotkeyToAction = new Dictionary<string, Func<Task>>();

        // Search all sections for hotkey entries
        foreach (var section in preferences.Sections)
        {
            foreach (var entry in section.Entries.Where(e => !string.IsNullOrEmpty(e.Value)))
            {
                // Map entry names to actions based on naming patterns
                var action = GetActionForHotkeyEntry(entry.Name);
                if (action != null)
                {
                    hotkeyToAction[entry.Value] = action;
                }
            }
        }

        // Create hotkey string from current key press
        var currentHotkey = CreateHotkeyString(keyInfo);

        // Check if this hotkey matches any configured hotkey
        if (hotkeyToAction.TryGetValue(currentHotkey, out var matchedAction))
        {
            await matchedAction();
            return true;
        }

        return false;
    }

    private Func<Task>? GetActionForHotkeyEntry(string entryName)
    {
        // Map entry names to actions based on specific naming patterns
        // Check for exit first
        if (entryName.Equals("Preferences.HotKeys.Exit", StringComparison.OrdinalIgnoreCase))
        {
            return () => { OnExitRequested(); return Task.CompletedTask; };
        }
        // Check for open preferences
        else if (entryName.Equals("Preferences.HotKeys.OpenPreferences", StringComparison.OrdinalIgnoreCase))
        {
            return OnPreferencesRequestedAsync;
        }
        // Check for show hotkeys
        else if (entryName.Equals("Preferences.HotKeys.ShowHotKeys", StringComparison.OrdinalIgnoreCase))
        {
            return OnHotKeysRequestedAsync;
        }

        return null; // Unknown hotkey type
    }

    private string CreateHotkeyString(ConsoleKeyInfo keyInfo)
    {
        var parts = new List<string>();

        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
            parts.Add("Ctrl");
        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt))
            parts.Add("Alt");
        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
            parts.Add("Shift");

        parts.Add(keyInfo.Key.ToString());

        return string.Join("+", parts);
    }

    private async Task OnPreferencesRequestedAsync()
    {
        try
        {
            _logger.LogDebug("Opening preferences editor");
            _renderer.RenderInfoMessage("Opening preferences editor...");

            await _preferencesManager.OpenPreferencesEditorAsync();
            _screenManager.RefreshLayout();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening preferences");
            _renderer.RenderErrorMessage($"Failed to open preferences: {ex.Message}");
        }
    }

    private async Task OnHotKeysRequestedAsync()
    {
        try
        {
            _logger.LogDebug("Displaying hotkey configuration");
            // Only display hotkey configuration, don't open preferences editor
            await _hotKeyService.ShowHotKeysAsync();
            _screenManager.RefreshLayout();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error displaying hotkey configuration");
            _renderer.RenderErrorMessage($"Failed to display hotkey configuration: {ex.Message}");
        }
    }

    private void OnPreferencesRequested()
    {
        _ = Task.Run(async () => await OnPreferencesRequestedAsync());
    }

    private void OnHotKeysRequested()
    {
        _ = Task.Run(async () => await OnHotKeysRequestedAsync());
    }

    private void OnExitRequested()
    {
        _logger.LogDebug("Exit requested");
        _isRunning = false;
    }

    private void OnMenuStateChanged()
    {
        // Screen refresh will happen in the main loop, no need to do it here
    }

    private void Cleanup()
    {
        _renderer.ClearScreen();
        _logger.LogInformation("Application shutdown complete");
    }
}
