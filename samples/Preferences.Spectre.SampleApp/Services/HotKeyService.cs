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
using Preferences.Common.SampleApp.Services;
using Spectre.Console;

namespace Preferences.Spectre.SampleApp.Services;

public class HotKeyService
{
    private readonly ILogger<HotKeyService> _logger;
    private readonly ConsoleRenderer _renderer;
    private readonly PreferencesManagerService _preferencesManager;
    private readonly ILocalizationService _localizationService;

    public HotKeyService(
        ILogger<HotKeyService> logger,
        ConsoleRenderer renderer,
        PreferencesManagerService preferencesManager,
        ILocalizationService localizationService)
    {
        _logger = logger;
        _renderer = renderer;
        _preferencesManager = preferencesManager;
        _localizationService = localizationService;
    }

    public async Task ShowHotKeysAsync()
    {
        try
        {
            _logger.LogDebug("Showing hot keys");

            _renderer.ClearScreen();
            _renderer.RenderSectionHeader("Keyboard Shortcuts", "Available hot keys and shortcuts");

            // Get all preferences from PreferencesOptions
            var preferences = _preferencesManager.GetCurrentPreferences();
            if (preferences?.Sections == null)
            {
                _renderer.RenderWarningMessage("No preferences configuration found");
                _renderer.PauseForUser();
                return;
            }

            // Find all sections that contain hotkey entries
            var hotKeySections = preferences.Sections
                .Where(section => section.Name.Contains("HotKey", StringComparison.OrdinalIgnoreCase) ||
                                 section.Entries.Any(entry => entry.Name.Contains("HotKey", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (!hotKeySections.Any())
            {
                _renderer.RenderWarningMessage("No hotkey configuration found in preferences");
                _renderer.PauseForUser();
                return;
            }

            _renderer.AddVerticalSpace(1);
            _renderer.RenderInfoMessage("Application shortcuts:");
            _renderer.AddVerticalSpace(1);

            // Display active shortcuts table
            var shortcutsTable = new Table();
            shortcutsTable.Border(TableBorder.Rounded);
            shortcutsTable.BorderColor(Color.FromConsoleColor(_renderer.CurrentTheme.BorderColor));

            // Add columns
            shortcutsTable.AddColumn(new TableColumn("Shortcut").Centered());
            shortcutsTable.AddColumn(new TableColumn("Action").LeftAligned());

            // No hardcoded shortcuts - all shortcuts come from preferences

            // Process all hotkey entries from all sections
            foreach (var section in hotKeySections)
            {
                foreach (var entry in section.Entries.Where(e => !string.IsNullOrEmpty(e.Value)))
                {
                    var actionDescription = _localizationService.GetLocalizedString(entry.Name);
                    var keyMarkup = $"[{_renderer.CurrentTheme.Primary}]{entry.Value}[/]";
                    var actionMarkup = $"[{_renderer.CurrentTheme.Success}]{actionDescription}[/]";
                    shortcutsTable.AddRow(keyMarkup, actionMarkup);
                }
            }

            AnsiConsole.Write(shortcutsTable);

            _renderer.AddVerticalSpace(2);
            _renderer.RenderInfoMessage("Press any key to return...");
            Console.ReadKey(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing hot keys");
            _renderer.RenderErrorMessage($"Failed to show hot keys: {ex.Message}");
            _renderer.PauseForUser();
        }
    }
}
