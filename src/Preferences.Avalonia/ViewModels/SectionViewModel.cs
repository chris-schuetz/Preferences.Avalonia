using Preferences.Avalonia.Models;
using ReactiveUI;

namespace Preferences.Avalonia.ViewModels;

public class SectionViewModel : ReactiveObject
{
    public SectionViewModel(PreferencesSection model)
    {
        Title = model.Title;
    }

    public string Title { get; }

    public List<EntryViewModel> Entries { get; } = new();
}
