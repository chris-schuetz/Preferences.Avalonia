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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Preferences.Common;
using Preferences.Common.SampleApp.Services;
using Preferences.Common.Services;
using Preferences.Spectre.SampleApp.Pages;
using Preferences.Spectre.SampleApp.Rendering;
using Spectre.Console;
using ZLogger;

namespace Preferences.Spectre.SampleApp;

internal sealed class Program
{
    // Add static service provider for accessing services from anywhere
    public static IServiceProvider? ServiceProvider { get; private set; }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var services = new ServiceCollection();
            ConfigureServices(services, configuration);

            using var serviceProvider = services.BuildServiceProvider();
            ServiceProvider = serviceProvider;

            var app = serviceProvider.GetRequiredService<InteractiveApplication>();

            return await app.RunAsync();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return -1;
        }
        finally
        {
            // Clear the static reference when done
            ServiceProvider = null;
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddZLoggerConsole(options =>
            {
                options.UsePlainTextFormatter(formatter =>
                {
                    formatter.SetPrefixFormatter($"[{0:HH:mm:ss}] ",
                        (in MessageTemplate template, in LogInfo info) =>
                            template.Format(info.Timestamp.Utc.DateTime));
                });
            });
        });

        services.Configure<PreferencesOptions>(options =>
            configuration.GetSection(PreferencesOptions.Preferences).Bind(options));

        services.AddSingleton<ILocalizationService, AppLocalizationService>();

        services.AddSingleton<ScreenLayout>();
        services.AddSingleton<IScreen, Screen>();
        services.AddSingleton<PreferencesPage>();
        services.AddSingleton<HotKeyPage>();
        services.AddSingleton<HotKeyService>();
        services.AddSingleton<InteractiveApplication>();
    }
}
