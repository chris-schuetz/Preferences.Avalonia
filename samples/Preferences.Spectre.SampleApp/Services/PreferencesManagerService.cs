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
using Preferences.Common.SampleApp.Services;
using Spectre.Console;

namespace Preferences.Spectre.SampleApp.Services;

public class PreferencesManagerService
{
    private readonly ILogger<PreferencesManagerService> _logger;
    private readonly ConsoleRenderer _renderer;
    private readonly IOptionsSnapshot<PreferencesOptions> _preferencesOptions;
    private readonly ILocalizationService _localizationService;

    public PreferencesManagerService(
        ILogger<PreferencesManagerService> logger,
        ConsoleRenderer renderer,
        IOptionsSnapshot<PreferencesOptions> preferencesOptions,
        ILocalizationService localizationService)
    {
        Console.WriteLine("PreferencesManagerService constructor called");
        _logger = logger;
        _renderer = renderer;
        _preferencesOptions = preferencesOptions;
        _localizationService = localizationService;
        Console.WriteLine("PreferencesManagerService constructor completed");
    }

    public async Task OpenPreferencesEditorAsync()
    {
        try
        {
            _logger.LogDebug("Opening preferences editor");

            var preferences = _preferencesOptions.Value;

            if (preferences?.Sections == null || !preferences.Sections.Any())
            {
                _renderer.ClearScreen();
                _renderer.RenderSectionHeader("Preferences Editor", "Configure application preferences");
                _renderer.RenderWarningMessage("No preferences sections found.");
                _renderer.PauseForUser();
                return;
            }

            // Start hierarchical navigation
            await NavigatePreferencesHierarchyAsync(preferences.Sections);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening preferences editor");
            _renderer.RenderErrorMessage($"Failed to open preferences editor: {ex.Message}");
            _renderer.PauseForUser();
        }
    }

    private async Task NavigatePreferencesHierarchyAsync(IEnumerable<PreferencesSection> sections)
    {
        var orderedSections = sections.OrderBy(s => s.Order).ToList();
        var hasUnsavedChanges = false;

        while (true)
        {
            // Step 1: Section selection with exit options
            var selectionResult = await SelectSectionAsync(orderedSections, hasUnsavedChanges);

            if (selectionResult.ChoiceType == SectionChoiceType.Cancel)
            {
                // User cancelled - exit without saving any unsaved changes
                if (hasUnsavedChanges)
                {
                    _renderer.ClearScreen();
                    _renderer.RenderSectionHeader("Preferences Editor", "Cancelled");
                    _renderer.RenderWarningMessage("All changes have been discarded.");
                    _renderer.PauseForUser();
                }
                break;
            }
            else if (selectionResult.ChoiceType == SectionChoiceType.SaveAndExit)
            {
                // User chose to save and exit
                if (hasUnsavedChanges)
                {
                    try
                    {
                        await SaveChangesToFileAsync();
                        ApplyThemeChanges();

                        _renderer.ClearScreen();
                        _renderer.RenderSectionHeader("Preferences Editor", "Saved Successfully");
                        _renderer.RenderSuccessMessage("All changes have been saved to appsettings.json");
                        _renderer.PauseForUser();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving preferences");
                        _renderer.RenderErrorMessage($"Failed to save preferences: {ex.Message}");
                        _renderer.PauseForUser();
                        continue; // Go back to section selection
                    }
                }
                break;
            }
            else if (selectionResult.Section != null)
            {
                // User selected a section to edit
                var sectionChanges = await NavigateSectionEntriesAsync(selectionResult.Section);
                hasUnsavedChanges = hasUnsavedChanges || sectionChanges;
            }
        }
    }

    private async Task<SectionSelectionResult> SelectSectionAsync(List<PreferencesSection> sections, bool hasUnsavedChanges)
    {
        _renderer.ClearScreen();
        _renderer.RenderSectionHeader("Preferences Editor", "Select a section to configure");

        // Create choices for section selection
        var sectionChoices = sections
            .Select(s => new SectionChoice(s, _localizationService.GetLocalizedString(s.Name), SectionChoiceType.Section))
            .ToList();

        // Add exit options
        if (hasUnsavedChanges)
        {
            sectionChoices.Add(new SectionChoice(null, "💾 Save & Exit", SectionChoiceType.SaveAndExit));
            sectionChoices.Add(new SectionChoice(null, "❌ Cancel (Discard Changes)", SectionChoiceType.Cancel));
        }
        else
        {
            sectionChoices.Add(new SectionChoice(null, "← Exit", SectionChoiceType.Cancel));
        }

        try
        {
            var selectedChoice = _renderer.PromptForChoice(
                "Select a preferences section:", 
                sectionChoices);

            return new SectionSelectionResult(selectedChoice.Section, selectedChoice.ChoiceType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in section selection");
            _renderer.RenderErrorMessage($"Error selecting section: {ex.Message}");
            return new SectionSelectionResult(null, SectionChoiceType.Cancel);
        }
    }

    private async Task<bool> NavigateSectionEntriesAsync(PreferencesSection section)
    {
        var hasChanges = false;
        var localizedSectionName = _localizationService.GetLocalizedString(section.Name);

        while (true)
        {
            // Show section overview and entry selection
            var selectedEntry = await SelectEntryAsync(section, localizedSectionName);
            if (selectedEntry == null)
            {
                // User chose to go back to section selection
                break;
            }

            // Edit the selected entry
            var entryChanged = await EditEntryAsync(selectedEntry, localizedSectionName);
            if (entryChanged)
            {
                hasChanges = true;
                // Changes are now saved explicitly when user chooses "Save & Exit"
            }
        }

        return hasChanges;
    }

    private async Task<PreferencesEntry?> SelectEntryAsync(PreferencesSection section, string sectionName)
    {
        _renderer.ClearScreen();
        _renderer.RenderSectionHeader($"Preferences Editor - {sectionName}", 
            "Select an entry to edit");

        if (section.Entries?.Any() != true)
        {
            _renderer.RenderWarningMessage("No entries found in this section.");
            _renderer.PauseForUser();
            return null;
        }

        // Display current values in a table
        DisplaySectionTable(section);
        _renderer.AddVerticalSpace(1);

        // Create choices for entry selection
        var entryChoices = section.Entries
            .Select(e => new EntryChoice(e, _localizationService.GetLocalizedString(e.Name)))
            .ToList();

        // Add a back option
        entryChoices.Add(new EntryChoice(null, "← Back to Section Selection"));

        try
        {
            var selectedChoice = _renderer.PromptForChoice(
                "Select an entry to edit:", 
                entryChoices);

            return selectedChoice.Entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in entry selection");
            _renderer.RenderErrorMessage($"Error selecting entry: {ex.Message}");
            return null;
        }
    }

    private async Task<bool> EditEntryAsync(PreferencesEntry entry, string sectionName = "")
    {
        var localizedEntryName = _localizationService.GetLocalizedString(entry.Name);
        var currentValue = entry.Value ?? "";

        _renderer.ClearScreen();
        _renderer.RenderSectionHeader($"Preferences Editor - {sectionName}", 
            $"Editing: {localizedEntryName}");

        // Show current value
        _renderer.RenderInfoMessage($"Current value: {currentValue}");

        if (entry.Options?.Any() == true)
        {
            _renderer.RenderInfoMessage($"Available options: {string.Join(", ", entry.Options)}");
        }

        _renderer.AddVerticalSpace(1);

        // Edit the value directly
        string newValue;
        try
        {
            if (entry.Options?.Any() == true)
            {
                // Use selection prompt for entries with predefined options
                var valueChoices = entry.Options.ToList();
                if (!valueChoices.Contains(currentValue) && !string.IsNullOrEmpty(currentValue))
                {
                    valueChoices.Insert(0, currentValue); // Add current value if not in options
                }

                // Add a back option
                valueChoices.Add("← Cancel");

                newValue = _renderer.PromptForChoice($"Select new value for '{localizedEntryName}':", valueChoices);

                if (newValue == "← Cancel")
                {
                    return false; // No changes made
                }
            }
            else
            {
                // Use text prompt for free-form entries
                newValue = _renderer.PromptForString($"Enter new value for '{localizedEntryName}':", currentValue);
            }

            if (newValue != currentValue)
            {
                entry.Value = newValue;
                _renderer.AddVerticalSpace(1);
                _renderer.RenderSuccessMessage($"Updated '{localizedEntryName}' from '{currentValue}' to '{newValue}'");
                _renderer.PauseForUser();
                return true; // Changes made
            }
            else
            {
                _renderer.AddVerticalSpace(1);
                _renderer.RenderInfoMessage("No changes made.");
                _renderer.PauseForUser();
                return false; // No changes made
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing entry value");
            _renderer.RenderErrorMessage($"Error editing value: {ex.Message}");
            _renderer.PauseForUser();
            return false; // No changes made
        }
    }

    // Helper classes for choices
    private class SectionChoice
    {
        public PreferencesSection? Section { get; }
        public string DisplayName { get; }
        public SectionChoiceType ChoiceType { get; }

        public SectionChoice(PreferencesSection? section, string displayName, SectionChoiceType choiceType)
        {
            Section = section;
            DisplayName = displayName;
            ChoiceType = choiceType;
        }

        public override string ToString() => DisplayName;
    }

    private enum SectionChoiceType
    {
        Section,
        Cancel,
        SaveAndExit
    }

    private class SectionSelectionResult
    {
        public PreferencesSection? Section { get; }
        public SectionChoiceType ChoiceType { get; }

        public SectionSelectionResult(PreferencesSection? section, SectionChoiceType choiceType)
        {
            Section = section;
            ChoiceType = choiceType;
        }
    }

    private class EntryChoice
    {
        public PreferencesEntry? Entry { get; }
        public string DisplayName { get; }

        public EntryChoice(PreferencesEntry? entry, string displayName)
        {
            Entry = entry;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }

    private async Task DisplayPreferencesSectionsAsync(IEnumerable<PreferencesSection> sections)
    {
        var orderedSections = sections.OrderBy(s => s.Order).ToList();
        var hasChanges = false;

        foreach (var section in orderedSections)
        {
            var sectionHasChanges = await DisplayPreferencesSectionAsync(section);
            hasChanges = hasChanges || sectionHasChanges;
        }

        _renderer.AddVerticalSpace(2);

        if (hasChanges)
        {
            _renderer.RenderSuccessMessage("Preferences have been updated and saved to appsettings.json");
        }
        else
        {
            _renderer.RenderInfoMessage("No changes were made to preferences.");
        }

        _renderer.PauseForUser();
    }

    private async Task<bool> DisplayPreferencesSectionAsync(PreferencesSection section)
    {
        var localizedName = _localizationService.GetLocalizedString(section.Name);
        var hasChanges = false;

        _renderer.RenderSubHeader(localizedName);
        _renderer.AddVerticalSpace(1);

        if (section.Entries?.Any() == true)
        {
            // Display current values first
            DisplaySectionTable(section);
            _renderer.AddVerticalSpace(1);

            // Ask if user wants to edit this section
            var editSection = _renderer.PromptForConfirmation($"Do you want to edit settings in '{localizedName}'?", false);

            if (editSection)
            {
                _renderer.AddVerticalSpace(1);

                foreach (var entry in section.Entries)
                {
                    var entryChanged = await EditEntryAsync(entry, localizedName);
                    hasChanges = hasChanges || entryChanged;
                }

                if (hasChanges)
                {
                    // Save changes to appsettings.json
                    await SaveChangesToFileAsync();

                    // Apply theme changes immediately
                    ApplyThemeChanges();
                }
            }
        }
        else
        {
            _renderer.RenderInfoMessage("No entries in this section.");
        }

        _renderer.AddVerticalSpace(2);
        return hasChanges;
    }

    private async Task AddPreferencesEntryToTableAsync(Table table, PreferencesEntry entry)
    {
        var localizedName = _localizationService.GetLocalizedString(entry.Name);
        var value = entry.Value ?? "[not set]";
        var options = entry.Options?.Any() == true 
            ? string.Join(", ", entry.Options) 
            : "[any value]";

        // Escape square brackets to prevent markup parsing errors
        var escapedValue = value.Replace("[", "[[").Replace("]", "]]");
        var escapedOptions = options.Replace("[", "[[").Replace("]", "]]");

        // Apply styling
        var nameMarkup = $"[{_renderer.CurrentTheme.Primary}]{localizedName.EscapeMarkup()}[/]";
        var valueMarkup = $"[{_renderer.CurrentTheme.Success}]{escapedValue}[/]";
        var optionsMarkup = $"[{_renderer.CurrentTheme.Muted}]{escapedOptions}[/]";

        table.AddRow(nameMarkup, valueMarkup, optionsMarkup);
    }

    public PreferencesOptions? GetCurrentPreferences()
    {
        return _preferencesOptions.Value;
    }

    public PreferencesSection? GetSection(string sectionName)
    {
        return _preferencesOptions.Value?.Sections?.FirstOrDefault(s => 
            string.Equals(s.Name, sectionName, StringComparison.OrdinalIgnoreCase));
    }

    public PreferencesEntry? GetEntry(string sectionName, string entryName)
    {
        try
        {
            Console.WriteLine($"GetEntry called with sectionName: {sectionName}, entryName: {entryName}");

            var preferencesValue = _preferencesOptions.Value;
            Console.WriteLine($"PreferencesOptions.Value is null: {preferencesValue == null}");

            if (preferencesValue?.Sections != null)
            {
                Console.WriteLine($"Number of sections: {preferencesValue.Sections.Count()}");
            }

            var section = GetSection(sectionName);
            Console.WriteLine($"Section found: {section != null}");

            if (section?.Entries != null)
            {
                Console.WriteLine($"Number of entries in section: {section.Entries.Count()}");
            }

            var entry = section?.Entries?.FirstOrDefault(e => 
                string.Equals(e.Name, entryName, StringComparison.OrdinalIgnoreCase));

            Console.WriteLine($"Entry found: {entry != null}");
            if (entry != null)
            {
                Console.WriteLine($"Entry value: {entry.Value}");
            }

            return entry;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetEntry: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    public string? GetEntryValue(string sectionName, string entryName)
    {
        var entry = GetEntry(sectionName, entryName);
        return entry?.Value;
    }

    private void DisplaySectionTable(PreferencesSection section)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.FromConsoleColor(_renderer.CurrentTheme.BorderColor));

        // Add columns
        table.AddColumn(new TableColumn("Setting").Centered());
        table.AddColumn(new TableColumn("Current Value").Centered());
        table.AddColumn(new TableColumn("Available Options").Centered());

        foreach (var entry in section.Entries)
        {
            AddPreferencesEntryToTableSync(table, entry);
        }

        AnsiConsole.Write(table);
    }

    private void AddPreferencesEntryToTableSync(Table table, PreferencesEntry entry)
    {
        var localizedName = _localizationService.GetLocalizedString(entry.Name);
        var value = entry.Value ?? "[[not set]]";
        var options = entry.Options?.Any() == true 
            ? string.Join(", ", entry.Options) 
            : "[[any value]]";

        // Apply styling
        var nameMarkup = $"[{_renderer.CurrentTheme.Primary}]{localizedName.EscapeMarkup()}[/]";
        var valueMarkup = $"[{_renderer.CurrentTheme.Success}]{value.EscapeMarkup()}[/]";
        var optionsMarkup = $"[{_renderer.CurrentTheme.Muted}]{options.EscapeMarkup()}[/]";

        table.AddRow(nameMarkup, valueMarkup, optionsMarkup);
    }


    private async Task SaveChangesToFileAsync()
    {
        try
        {
            // Get the current preferences
            var preferences = _preferencesOptions.Value;
            if (preferences == null) return;

            // Save to appsettings.json using the AppSettings helper
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

            // Create a wrapper object with the Preferences section
            var wrapper = new { Preferences = preferences };

            // Use AppSettings.Update to save changes
            Common.SampleApp.AppSettings.Update(wrapper, "", appSettingsPath);

            _renderer.RenderSuccessMessage("Preferences saved to appsettings.json");
            _logger.LogInformation("Preferences saved to {AppSettingsPath}", appSettingsPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save preferences to file");
            _renderer.RenderErrorMessage($"Failed to save preferences: {ex.Message}");
        }
    }

    private void ApplyThemeChanges()
    {
        try
        {
            // Get the current theme setting
            var themeEntry = GetEntry("Preferences.General", "Preferences.General.Theme");
            if (themeEntry?.Value != null)
            {
                _renderer.UpdateTheme(themeEntry.Value);
                _logger.LogDebug("Applied theme change: {Theme}", themeEntry.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply theme changes");
        }
    }
}
