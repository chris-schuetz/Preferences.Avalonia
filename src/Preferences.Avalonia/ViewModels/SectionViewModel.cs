using System.Collections.ObjectModel;
using DynamicData;
using Preferences.Avalonia.Models;
using ReactiveUI;

namespace Preferences.Avalonia.ViewModels;

public class SectionViewModel : ReactiveObject
{
    public SectionViewModel(PreferencesSection model)
    {
        Title = model.Title;
        Entries = new ObservableCollection<EntryViewModel>(model.Entries.Select(e => new EntryViewModel(e)));
    }

    public string Title { get; }

    public ObservableCollection<EntryViewModel> Entries { get; }
}
