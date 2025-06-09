using Preferences.Avalonia.Models;
using ReactiveUI;

namespace Preferences.Avalonia.ViewModels;

public class EntryViewModel(EntryModel model) : ReactiveObject
{
    private EntryModel _model = model;

    public string Title => _model.Title;

    public string Value
    {
        get => _model.Value;
        set => _model.Value = value;
    }
}
