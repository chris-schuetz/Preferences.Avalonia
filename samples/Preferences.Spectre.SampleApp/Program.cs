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
using Microsoft.Extensions.Options;
using Preferences.Common;
using Preferences.Common.SampleApp.Services;
using Preferences.Spectre.SampleApp.Services;
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
            Console.WriteLine("Starting application...");

            // Build configuration
            Console.WriteLine("Building configuration...");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            Console.WriteLine("Configuration built successfully.");

            // Build service collection
            Console.WriteLine("Configuring services...");
            var services = new ServiceCollection();
            ConfigureServices(services, configuration);

            Console.WriteLine("Services configured successfully.");

            // Build service provider
            Console.WriteLine("Building service provider...");
            using var serviceProvider = services.BuildServiceProvider();

            // Store the service provider for static access
            ServiceProvider = serviceProvider;

            Console.WriteLine("Service provider built successfully.");

            // Get the interactive application service
            Console.WriteLine("Getting InteractiveApplication service...");
            var app = serviceProvider.GetRequiredService<InteractiveApplication>();

            Console.WriteLine("InteractiveApplication service obtained successfully.");

            // Run the interactive application
            Console.WriteLine("Starting interactive application...");
            return await app.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error in Main: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
        try
        {
            Console.WriteLine("  Adding configuration...");
            // Add configuration
            services.AddSingleton(configuration);

            Console.WriteLine("  Adding logging...");
            // Add logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddZLoggerConsole(options =>
                {
                    options.UsePlainTextFormatter(formatter =>
                    {
                        formatter.SetPrefixFormatter($"[{0:HH:mm:ss}] ",
                            (in MessageTemplate template, in LogInfo info) => template.Format(info.Timestamp.Utc.DateTime));
                    });
                });
            });

            Console.WriteLine("  Adding preferences configuration...");
            // Add preferences configuration
            services.Configure<PreferencesOptions>(options =>
                configuration.GetSection(PreferencesOptions.Preferences).Bind(options));

            Console.WriteLine("  Adding common services...");
            // Add common services
            services.AddSingleton<ILocalizationService, AppLocalizationService>();

            Console.WriteLine("  Adding Spectre.Console services...");
            // Add Spectre.Console services
            Console.WriteLine("    Adding ConsoleRenderer...");
            services.AddSingleton<ConsoleRenderer>();

            Console.WriteLine("    Adding ScreenManager...");
            services.AddSingleton<ScreenManager>();

            Console.WriteLine("    Adding PreferencesManagerService...");
            services.AddSingleton<PreferencesManagerService>();

            Console.WriteLine("    Adding HotKeyService...");
            services.AddSingleton<HotKeyService>();

            Console.WriteLine("    Adding InteractiveApplication...");
            services.AddSingleton<InteractiveApplication>();

            Console.WriteLine("  All services configured successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ConfigureServices: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
