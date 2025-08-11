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
using ReactiveUI;

namespace Preferences.Avalonia.ViewModels;

public class PreferencesViewModel : ReactiveObject
{
    private readonly PreferencesOptions _preferencesOptions;
    private SectionViewModel _selectedSection;

    public PreferencesViewModel() : this(new PreferencesOptions())
    {
    }

    public PreferencesViewModel(PreferencesOptions preferencesOptions)
    {
        _preferencesOptions = preferencesOptions;

        // Convert model sections to view model sections
        Sections = new List<SectionViewModel>();
        foreach (var section in _preferencesOptions.Sections) Sections.Add(new SectionViewModel(section));
        SelectedSection = Sections[0];
    }

    public List<SectionViewModel> Sections { get; }

    public SectionViewModel SelectedSection
    {
        get => _selectedSection;
        set => this.RaiseAndSetIfChanged(ref _selectedSection, value);
    }

    public PreferencesOptions Options => _preferencesOptions;
}
