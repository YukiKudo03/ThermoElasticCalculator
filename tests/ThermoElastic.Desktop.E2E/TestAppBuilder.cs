using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Fluent;

[assembly: AvaloniaTestApplication(typeof(ThermoElastic.Desktop.E2E.TestAppBuilder))]

namespace ThermoElastic.Desktop.E2E;

/// <summary>
/// Configures the Avalonia headless test application with Skia rendering enabled
/// for screenshot capture and visual UI testing.
/// </summary>
public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = false // Enable real pixel rendering for screenshots
            });
}

/// <summary>
/// Minimal test application with Fluent theme and DataGrid styles.
/// Mirrors the production App configuration.
/// </summary>
public class TestApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();
    }
}
