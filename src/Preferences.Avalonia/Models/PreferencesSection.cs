namespace Preferences.Avalonia.Models;

public class PreferencesSection
{
    public required string Name { get; init; }

    public required string Title { get; set; }

    public required int Order { get; set; }

    public required List<EntryModel> Entries { get; set; }
}
