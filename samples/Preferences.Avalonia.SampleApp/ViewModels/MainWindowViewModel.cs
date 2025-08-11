// Copyright (c) 2025 Christopher Schütz
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

using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Input;
using Microsoft.Extensions.Options;
using Preferences.Avalonia.Models;
using Preferences.Avalonia.SampleApp.Services;
using Preferences.Avalonia.ViewModels;
using ReactiveUI;

namespace Preferences.Avalonia.SampleApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _greeting = "Welcome to Avalonia!";
    private PreferencesOptions _preferencesOptions;

    public MainWindowViewModel(IOptions<PreferencesOptions> hotKeyOptions)
    {
        _preferencesOptions = hotKeyOptions.Value;
        OpenPreferencesDialog = ReactiveCommand.CreateFromTask(async () =>
        {
            PreferencesOptions = await ShowPreferencesDialog.Handle(new PreferencesViewModel(PreferencesOptions, new AppLocalizationService()));
            ConfigurationUpdater.UpdateAppSettings(PreferencesOptions, PreferencesOptions.Preferences);
        });
    }

    public ICommand OpenPreferencesDialog { get; }

    public KeyGesture? OpenPreferencesGesture
    {
        get
        {
            return PreferencesOptions.Sections.FirstOrDefault(s => s.Name == "Preferences.HotKeys")?.Entries
                .FirstOrDefault(hk => hk.Name == "Preferences.HotKeys.OpenPreferences") is { } open
                ? KeyGesture.Parse(open.Value)
                : null;
        }
    }

    public IInteraction<PreferencesViewModel, PreferencesOptions> ShowPreferencesDialog { get; } =
        new Interaction<PreferencesViewModel, PreferencesOptions>();

    public string Greeting
    {
        get => _greeting;
        set
        {
            _greeting = value;
            this.RaisePropertyChanged();
        }
    }

    public PreferencesOptions PreferencesOptions
    {
        get => _preferencesOptions;
        set
        {
            _preferencesOptions = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(OpenPreferencesGesture));
        }
    }
}
