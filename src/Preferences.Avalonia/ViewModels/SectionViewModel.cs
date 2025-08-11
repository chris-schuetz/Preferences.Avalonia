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

using System.Collections.ObjectModel;
using Preferences.Avalonia.Models;
using Preferences.Avalonia.Services;
using ReactiveUI;

namespace Preferences.Avalonia.ViewModels;

public class SectionViewModel : ReactiveObject, IDisposable
{
    private readonly ILocalizationService? _localizationService;
    private bool _isDisposed;

    public SectionViewModel(PreferencesSection model, ILocalizationService? localizationService = null)
    {
        Model = model;
        _localizationService = localizationService;
        Entries = new ObservableCollection<EntryViewModel>(model.Entries.Select(e =>
            new EntryViewModel(e, localizationService)));

        if (_localizationService != null) _localizationService.LocaleChanged += OnLocaleChanged;
    }

    public string Title => _localizationService != null
        ? _localizationService.GetLocalizedString(Model.Name)
        : Model.Name;

    public ObservableCollection<EntryViewModel> Entries { get; }

    public PreferencesSection Model { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SectionViewModel()
    {
        Dispose(false);
    }

    private void OnLocaleChanged(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(Title));
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
            if (_localizationService != null)
                _localizationService.LocaleChanged -= OnLocaleChanged;

        _isDisposed = true;
    }
}
