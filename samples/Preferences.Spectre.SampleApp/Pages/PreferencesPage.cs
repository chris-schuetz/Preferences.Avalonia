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
using Preferences.Common.SampleApp;
using Preferences.Common.Services;
using Preferences.Spectre.SampleApp.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Preferences.Spectre.SampleApp.Pages;

public class PreferencesPage : IPage
{
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<PreferencesPage> _logger;
    private readonly IOptionsMonitor<PreferencesOptions> _preferencesOptions;

    public PreferencesPage(
        ILogger<PreferencesPage> logger,
        IOptionsMonitor<PreferencesOptions> preferencesOptions,
        ILocalizationService localizationService)
    {
        _logger = logger;
        _logger.LogEntry();
        _preferencesOptions = preferencesOptions;
        _localizationService = localizationService;
        _logger.LogExit();
    }

    public IRenderable Draw(ScreenLayout screenLayout)
    {
        var rows = new List<IRenderable>();
        rows.Add(screenLayout.GetHeader(_localizationService.GetLocalizedString("Preferences")));

        foreach (var section in _preferencesOptions.CurrentValue.Sections.OrderBy(s => s.Order))
        {
            rows.Add(screenLayout.GetSubHeader(_localizationService.GetLocalizedString(section.Name)));
            foreach (var entry in section.Entries)
                rows.Add(screenLayout.GetContent(
                    $"{_localizationService.GetLocalizedString(entry.Name)}: {entry.Value}"));
        }

        return new Rows(rows);
    }
}
