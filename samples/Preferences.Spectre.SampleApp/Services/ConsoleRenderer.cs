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
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Preferences.Spectre.SampleApp.Services;

public class ConsoleRenderer
{
    private readonly ILogger<ConsoleRenderer> _logger;
    private ConsoleTheme _currentTheme;

    public ConsoleRenderer(ILogger<ConsoleRenderer> logger)
    {
        Console.WriteLine("ConsoleRenderer constructor called");
        _logger = logger;
        Console.WriteLine("Creating system theme...");
        _currentTheme = CreateSystemTheme(); // Start with system theme
        Console.WriteLine("ConsoleRenderer constructor completed successfully");
    }

    public ConsoleTheme CurrentTheme => _currentTheme;

    public void ClearScreen()
    {
        AnsiConsole.Clear();
    }

    public void RenderMainHeader(string title)
    {
        var rule = new Rule($"[{CurrentTheme.Primary}]{title}[/]")
        {
            Style = new Style(foreground: Color.FromConsoleColor(CurrentTheme.PrimaryColor)),
            Justification = Justify.Center
        };

        AnsiConsole.Write(rule);
    }

    public void RenderSectionHeader(string title, string? description = null)
    {
        var panel = new Panel(CreateHeaderContent(title, description))
        {
            Border = BoxBorder.Double,
            BorderStyle = new Style(foreground: Color.FromConsoleColor(_currentTheme.PrimaryColor)),
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
        AddVerticalSpace(1);
    }

    public void RenderSubHeader(string title)
    {
        AnsiConsole.MarkupLine($"[{_currentTheme.Secondary}]{title}[/]");

        var rule = new Rule()
        {
            Style = new Style(foreground: Color.FromConsoleColor(_currentTheme.MutedColor))
        };

        AnsiConsole.Write(rule);
    }

    public void RenderThinSeparator()
    {
        var rule = new Rule()
        {
            Style = new Style(foreground: Color.FromConsoleColor(_currentTheme.BorderColor))
        };

        AnsiConsole.Write(rule);
    }

    public void RenderStatusBar(string message, string shortcuts)
    {
        var content = new Grid();
        content.AddColumn(new GridColumn().NoWrap());
        content.AddColumn(new GridColumn().NoWrap());

        content.AddRow(
            new Markup($"[{_currentTheme.Muted}]{message}[/]"),
            new Markup($"[{_currentTheme.Info}]{shortcuts}[/]")
        );

        var panel = new Panel(content)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: Color.FromConsoleColor(_currentTheme.BorderColor)),
            Padding = new Padding(1, 0)
        };

        AnsiConsole.Write(panel);
    }

    public void RenderInfoMessage(string message)
    {
        AnsiConsole.MarkupLine($"[{_currentTheme.Info}]ℹ {message}[/]");
    }

    public void RenderSuccessMessage(string message)
    {
        AnsiConsole.MarkupLine($"[{_currentTheme.Success}]✓ {message}[/]");
    }

    public void RenderWarningMessage(string message)
    {
        AnsiConsole.MarkupLine($"[{_currentTheme.Warning}]⚠ {message}[/]");
    }

    public void RenderErrorMessage(string message)
    {
        AnsiConsole.MarkupLine($"[{_currentTheme.Error}]✗ {message}[/]");
    }

    public void AddVerticalSpace(int lines = 1)
    {
        for (int i = 0; i < lines; i++)
        {
            AnsiConsole.WriteLine();
        }
    }

    public void PauseForUser(string message = "Press any key to continue...")
    {
        AnsiConsole.MarkupLine($"[{_currentTheme.Muted}]{message}[/]");
        Console.ReadKey(true);
    }

    public string PromptForString(string prompt, string? defaultValue = null)
    {
        var textPrompt = new TextPrompt<string>($"[{_currentTheme.Primary}]{prompt}[/]");

        if (!string.IsNullOrEmpty(defaultValue))
        {
            textPrompt.DefaultValue(defaultValue);
            textPrompt.ShowDefaultValue = true;
        }

        return AnsiConsole.Prompt(textPrompt);
    }

    public T PromptForChoice<T>(string prompt, IEnumerable<T> choices) where T : notnull
    {
        var selectionPrompt = new SelectionPrompt<T>()
            .Title($"[{_currentTheme.Primary}]{prompt}[/]")
            .AddChoices(choices);

        return AnsiConsole.Prompt(selectionPrompt);
    }

    public bool PromptForConfirmation(string prompt, bool defaultValue = false)
    {
        var confirmPrompt = new ConfirmationPrompt($"[{_currentTheme.Primary}]{prompt}[/]")
        {
            DefaultValue = defaultValue
        };

        return AnsiConsole.Prompt(confirmPrompt);
    }

    public void RenderTable<T>(IEnumerable<T> items, Action<Table, T> configureRow, params string[] headers)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.FromConsoleColor(_currentTheme.BorderColor));

        // Add headers
        foreach (var header in headers)
        {
            table.AddColumn(new TableColumn($"[{_currentTheme.Secondary}]{header}[/]").Centered());
        }

        // Add rows
        foreach (var item in items)
        {
            configureRow(table, item);
        }

        AnsiConsole.Write(table);
    }

    public void RenderProgress(string description, Action<ProgressContext> action)
    {
        AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn()
            )
            .Start(ctx => action(ctx));
    }

    private IRenderable CreateHeaderContent(string title, string? description)
    {
        var content = new List<IRenderable>
        {
            new Markup($"[{_currentTheme.Primary}]{title}[/]")
        };

        if (!string.IsNullOrEmpty(description))
        {
            content.Add(new Markup($"[{_currentTheme.Muted}]{description}[/]"));
        }

        return new Rows(content);
    }

    public void SetCursorPosition(int left, int top)
    {
        try
        {
            Console.SetCursorPosition(left, top);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set cursor position to ({Left}, {Top})", left, top);
        }
    }

    public (int Left, int Top) GetCursorPosition()
    {
        try
        {
            return Console.GetCursorPosition();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get cursor position");
            return (0, 0);
        }
    }

    public void ShowException(Exception exception)
    {
        AnsiConsole.WriteException(exception);
    }

    public void UpdateTheme(string themeName)
    {
        _logger.LogDebug("Updating theme to: {ThemeName}", themeName);

        _currentTheme = themeName.ToLowerInvariant() switch
        {
            "light" => CreateLightTheme(),
            "dark" => CreateDarkTheme(),
            "system" => CreateSystemTheme(),
            _ => CreateSystemTheme()
        };

        _logger.LogInformation("Theme updated to: {ThemeName}", themeName);
    }

    private static ConsoleTheme CreateLightTheme()
    {
        return new ConsoleTheme
        {
            Primary = "blue",
            Secondary = "navy",
            Success = "darkgreen",
            Warning = "orange3",
            Error = "red",
            Info = "black",
            Muted = "grey",
            Border = "grey",
            Background = "white",

            PrimaryColor = ConsoleColor.Blue,
            SecondaryColor = ConsoleColor.DarkCyan,
            SuccessColor = ConsoleColor.DarkGreen,
            WarningColor = ConsoleColor.DarkYellow,
            ErrorColor = ConsoleColor.Red,
            InfoColor = ConsoleColor.Black,
            MutedColor = ConsoleColor.Gray,
            BorderColor = ConsoleColor.Gray,
            BackgroundColor = ConsoleColor.White
        };
    }

    private static ConsoleTheme CreateDarkTheme()
    {
        return new ConsoleTheme
        {
            Primary = "cyan",
            Secondary = "blue",
            Success = "green",
            Warning = "yellow",
            Error = "red",
            Info = "white",
            Muted = "grey",
            Border = "grey",
            Background = "black",

            PrimaryColor = ConsoleColor.Cyan,
            SecondaryColor = ConsoleColor.Blue,
            SuccessColor = ConsoleColor.Green,
            WarningColor = ConsoleColor.Yellow,
            ErrorColor = ConsoleColor.Red,
            InfoColor = ConsoleColor.White,
            MutedColor = ConsoleColor.Gray,
            BorderColor = ConsoleColor.DarkGray,
            BackgroundColor = ConsoleColor.Black
        };
    }

    private static ConsoleTheme CreateSystemTheme()
    {
        // Try to detect system theme, fallback to dark
        return CreateDarkTheme();
    }
}
