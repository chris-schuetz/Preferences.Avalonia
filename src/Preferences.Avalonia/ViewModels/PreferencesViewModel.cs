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

using Preferences.Avalonia.Models;
using Preferences.Avalonia.Services;
using ReactiveUI;

namespace Preferences.Avalonia.ViewModels;

public class PreferencesViewModel : ReactiveObject
{
    private readonly ILocalizationService? _localizationService;
    private SectionViewModel? _selectedSection;

    public PreferencesViewModel(PreferencesOptions preferencesOptions, ILocalizationService? localizationService = null)
    {
        _localizationService = localizationService;
        Options = preferencesOptions;

        // Convert model sections to view model sections
        Sections = new List<SectionViewModel>();
        foreach (var section in Options.Sections) Sections.Add(new SectionViewModel(section, _localizationService));

        Sections = Sections.OrderBy(s => s.Model.Order).ToList();
        SelectedSection = Sections.FirstOrDefault();
    }

    public List<SectionViewModel> Sections { get; }

    public SectionViewModel? SelectedSection
    {
        get => _selectedSection;
        set => this.RaiseAndSetIfChanged(ref _selectedSection, value);
    }

    public PreferencesOptions Options { get; }
}
