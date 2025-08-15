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
using Preferences.Common.Messages;
using Preferences.Common.SampleApp;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Preferences.Spectre.SampleApp.Rendering;

public class ScreenLayout
{
    private readonly ILogger<ScreenLayout> _logger;

    public ScreenLayout(ILogger<ScreenLayout> logger)
    {
        _logger = logger;
        CurrentTheme = Theme.CreateDarkTheme();
    }

    public Theme CurrentTheme { get; set; }

    public void ClearScreen()
    {
        _logger.LogEntry();
        AnsiConsole.Clear();
        _logger.LogExit();
    }

    public void RenderMainHeader(string title)
    {
        _logger.LogEntry();
        Console.SetCursorPosition(0, 0);
        var rule = new Rule(title)
        {
            Style = GetPrimaryStyle(),
            Justification = Justify.Center
        };

        AnsiConsole.Write(rule);
        AddVerticalSpace(1);
        _logger.LogExit();
    }

    public IRenderable GetSectionHeader(string title, string? description = null)
    {
        return new Panel(CreateHeaderContent(title, description))
        {
            Border = BoxBorder.Double,
            BorderStyle = GetPrimaryStyle(),
            Padding = new Padding(2, 1)
        };
    }

    public IRenderable GetSubHeader(string title)
    {
        var subHeader = new Markup(title, GetSecondaryStyle());
        var rule = new Rule { Style = GetMutedStyle() };
        return new Rows(subHeader, rule);
    }

    public IRenderable GetThinSeparator()
    {
        return new Rule { Style = GetBorderStyle() };
    }

    public void RenderMainContent(IRenderable? currentPageContent)
    {
        Console.SetCursorPosition(0, 3);
        AnsiConsole.Write(currentPageContent ?? new Markup("No content", GetMutedStyle()));;

        (_, int row) = Console.GetCursorPosition();
        for (var i = row; i < Console.WindowHeight - 6; i++)
        {
            AnsiConsole.WriteLine();
        }
    }

    public void ClearMainContent()
    {
        Console.SetCursorPosition(0, 3);
        for (var i = 0; i < Console.WindowHeight - 6; i++)
        {
            AnsiConsole.WriteLine();
        }
    }

    public void RenderStatusBar(string hint, string status, StatusMessageType statusType)
    {
        Console.SetCursorPosition(0, Console.WindowHeight - 4);
        var content = new Grid();
        content.AddColumn(new GridColumn().NoWrap());
        content.AddColumn(new GridColumn().NoWrap());

        content.AddRow(
            new Markup(hint, new Style(Color.FromConsoleColor(CurrentTheme.MutedColor))),
            new Markup(status, new Style(GetStatusColor(statusType)))
        );

        var panel = new Panel(content)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.FromConsoleColor(CurrentTheme.BorderColor)),
            Padding = new Padding(1, 0)
        };

        AnsiConsole.Write(panel);
    }

    public void UpdateTheme(string themeName)
    {
        _logger.LogDebug("Updating theme to: {ThemeName}", themeName);

        CurrentTheme = themeName.ToLowerInvariant() switch
        {
            "light" => Theme.CreateLightTheme(),
            "dark" => Theme.CreateDarkTheme(),
            _ => Theme.CreateDarkTheme()
        };

        _logger.LogInformation("Theme updated to: {ThemeName}", themeName);
    }

    private Color GetStatusColor(StatusMessageType messageType)
    {
        return messageType switch
        {
            StatusMessageType.Info => Color.FromConsoleColor(CurrentTheme.InfoColor),
            StatusMessageType.Success => Color.FromConsoleColor(CurrentTheme.SuccessColor),
            StatusMessageType.Warning => Color.FromConsoleColor(CurrentTheme.WarningColor),
            StatusMessageType.Error => Color.FromConsoleColor(CurrentTheme.ErrorColor),
            _ => Color.FromConsoleColor(CurrentTheme.MutedColor)
        };
    }

    private void AddVerticalSpace(int lines)
    {
        for (var i = 0; i < lines; i++)
        {
            AnsiConsole.WriteLine();
        }
    }

    private IRenderable CreateHeaderContent(string title, string? description)
    {
        var content = new List<IRenderable>
        {
            new Markup(title, GetPrimaryStyle())
        };

        if (!string.IsNullOrEmpty(description))
        {
            content.Add(new Markup(description, GetMutedStyle()));
        }

        return new Rows(content);
    }


    private Style GetPrimaryStyle()
    {
        return new Style(Color.FromConsoleColor(CurrentTheme.PrimaryColor),
            Color.FromConsoleColor(CurrentTheme.BackgroundColor));
    }

    private Style GetSecondaryStyle()
    {
        return new Style(Color.FromConsoleColor(CurrentTheme.SecondaryColor),
            Color.FromConsoleColor(CurrentTheme.BackgroundColor));
    }

    private Style GetMutedStyle()
    {
        return new Style(Color.FromConsoleColor(CurrentTheme.MutedColor),
            Color.FromConsoleColor(CurrentTheme.BackgroundColor));
    }

    private Style GetBorderStyle()
    {
        return new Style(Color.FromConsoleColor(CurrentTheme.BorderColor),
            Color.FromConsoleColor(CurrentTheme.BackgroundColor));
    }
}
