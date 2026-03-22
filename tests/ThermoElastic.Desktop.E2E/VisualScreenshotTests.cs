using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ThermoElastic.Desktop.ViewModels;
using ThermoElastic.Desktop.Views;
using Xunit;
using Xunit.Abstractions;

namespace ThermoElastic.Desktop.E2E;

/// <summary>
/// Visual UI tests that capture screenshots of each view for inspection.
/// Screenshots are saved to tests/ThermoElastic.Desktop.E2E/screenshots/.
/// Use these to verify layout, sizing, and visual quality of each view.
/// </summary>
public class VisualScreenshotTests
{
    private readonly ITestOutputHelper _output;
    private static readonly string ScreenshotDir;

    static VisualScreenshotTests()
    {
        // Save screenshots relative to the project root
        ScreenshotDir = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "screenshots");
        Directory.CreateDirectory(ScreenshotDir);
    }

    public VisualScreenshotTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Render a view inside a Window and capture a screenshot.
    /// </summary>
    private string CaptureView(string name, Control content, int width = 880, int height = 650)
    {
        var window = new Window
        {
            Width = width,
            Height = height,
            Content = content,
        };

        window.Show();
        // Force layout pass
        Dispatcher.UIThread.RunJobs();
        window.InvalidateArrange();
        window.InvalidateMeasure();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var frame = window.CaptureRenderedFrame();
        var path = Path.Combine(ScreenshotDir, $"{name}.png");
        frame?.Save(path);
        window.Close();

        _output.WriteLine($"Screenshot saved: {path} ({frame?.PixelSize.Width}x{frame?.PixelSize.Height})");
        return path;
    }

    /// <summary>
    /// Capture the MainWindow directly (it is a Window, not a UserControl).
    /// </summary>
    [AvaloniaFact]
    public void Capture_MainWindow_WithNavigation()
    {
        var mainWindow = new MainWindow
        {
            Width = 1100,
            Height = 750,
        };
        mainWindow.Show();
        Dispatcher.UIThread.RunJobs();
        mainWindow.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var frame = mainWindow.CaptureRenderedFrame();
        var path = Path.Combine(ScreenshotDir, "00_MainWindow.png");
        frame?.Save(path);
        mainWindow.Close();

        _output.WriteLine($"Screenshot saved: {path} ({frame?.PixelSize.Width}x{frame?.PixelSize.Height})");
        Assert.True(File.Exists(path), $"Screenshot should exist at {path}");
    }

    // =================================================================
    // Core Tools
    // =================================================================

    [AvaloniaFact]
    public void Capture_HugoniotView_WithResults()
    {
        var vm = new HugoniotViewModel();
        vm.SelectedMineralIndex = 0; // Forsterite
        vm.NumPoints = 10;
        vm.CalculateCommand.Execute(null);

        var view = new HugoniotView { DataContext = vm };
        var path = CaptureView("01_Hugoniot", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_PhaseDiagramView()
    {
        var vm = new PhaseDiagramExplorerViewModel();
        var view = new PhaseDiagramExplorerView { DataContext = vm };
        var path = CaptureView("02_PhaseDiagram", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_LookupTableView()
    {
        var vm = new LookupTableViewModel();
        var view = new LookupTableView { DataContext = vm };
        var path = CaptureView("03_LookupTable", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_SensitivityKernelView_WithResults()
    {
        var vm = new SensitivityKernelViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new SensitivityKernelView { DataContext = vm };
        var path = CaptureView("04_SensitivityKernel", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_PlanetaryInteriorView_WithResults()
    {
        var vm = new PlanetaryInteriorViewModel();
        vm.SelectedPlanetIndex = 0; // Earth
        vm.CalculateCommand.Execute(null);
        var view = new PlanetaryInteriorView { DataContext = vm };
        var path = CaptureView("05_PlanetaryInterior_Earth", view);
        Assert.True(File.Exists(path));
    }

    // =================================================================
    // EOS & Shock
    // =================================================================

    [AvaloniaFact]
    public void Capture_EOSFitterView()
    {
        var vm = new EOSFitterViewModel();
        var view = new EOSFitterView { DataContext = vm };
        var path = CaptureView("06_EOSFitter", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_VerificationDashboardView_WithResults()
    {
        var vm = new VerificationDashboardViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new VerificationDashboardView { DataContext = vm };
        var path = CaptureView("07_VerificationDashboard", view);
        Assert.True(File.Exists(path));
    }

    // =================================================================
    // Phase Equilibria
    // =================================================================

    [AvaloniaFact]
    public void Capture_PostPerovskiteView_WithResults()
    {
        var vm = new PostPerovskiteViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new PostPerovskiteView { DataContext = vm };
        var path = CaptureView("08_PostPerovskite", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_ClassicalGeobarometryView()
    {
        var vm = new ClassicalGeobarometryViewModel();
        var view = new ClassicalGeobarometryView { DataContext = vm };
        var path = CaptureView("09_ClassicalGeobarometry", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_GeobarometryView()
    {
        var vm = new GeobarometryViewModel();
        var view = new GeobarometryView { DataContext = vm };
        var path = CaptureView("10_ElasticGeobarometry", view);
        Assert.True(File.Exists(path));
    }

    // =================================================================
    // Mantle & Deep Earth
    // =================================================================

    [AvaloniaFact]
    public void Capture_AnelasticityView_WithResults()
    {
        var vm = new AnelasticityViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new AnelasticityView { DataContext = vm };
        var path = CaptureView("11_Anelasticity", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_LLSVPView_WithResults()
    {
        var vm = new LLSVPViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new LLSVPView { DataContext = vm };
        var path = CaptureView("12_LLSVP", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_ULVZView_WithResults()
    {
        var vm = new ULVZViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new ULVZView { DataContext = vm };
        var path = CaptureView("13_ULVZ", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_SlabModelView_WithResults()
    {
        var vm = new SlabModelViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new SlabModelView { DataContext = vm };
        var path = CaptureView("14_SlabModel", view);
        Assert.True(File.Exists(path));
    }

    // =================================================================
    // Material Properties
    // =================================================================

    [AvaloniaFact]
    public void Capture_SpinCrossoverView_WithResults()
    {
        var vm = new SpinCrossoverViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new SpinCrossoverView { DataContext = vm };
        var path = CaptureView("15_SpinCrossover", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_ThermalConductivityView_WithResults()
    {
        var vm = new ThermalConductivityViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new ThermalConductivityView { DataContext = vm };
        var path = CaptureView("16_ThermalConductivity", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_ElectricalConductivityView_WithResults()
    {
        var vm = new ElectricalConductivityViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new ElectricalConductivityView { DataContext = vm };
        var path = CaptureView("17_ElectricalConductivity", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_ElasticTensorView_WithResults()
    {
        var vm = new ElasticTensorViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new ElasticTensorView { DataContext = vm };
        var path = CaptureView("18_ElasticTensor", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_OxygenFugacityView_WithResults()
    {
        var vm = new OxygenFugacityViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new OxygenFugacityView { DataContext = vm };
        var path = CaptureView("19_OxygenFugacity", view);
        Assert.True(File.Exists(path));
    }

    // =================================================================
    // Composition & Fluids
    // =================================================================

    [AvaloniaFact]
    public void Capture_CompositionInverterView()
    {
        var vm = new CompositionInverterViewModel();
        var view = new CompositionInverterView { DataContext = vm };
        var path = CaptureView("20_CompositionInverter", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_IronPartitioningView_WithResults()
    {
        var vm = new IronPartitioningViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new IronPartitioningView { DataContext = vm };
        var path = CaptureView("21_IronPartitioning", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_WaterContentView_WithResults()
    {
        var vm = new WaterContentViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new WaterContentView { DataContext = vm };
        var path = CaptureView("22_WaterContent", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_MagmaOceanView_WithResults()
    {
        var vm = new MagmaOceanViewModel();
        vm.CalculateCommand.Execute(null);
        var view = new MagmaOceanView { DataContext = vm };
        var path = CaptureView("23_MagmaOcean", view);
        Assert.True(File.Exists(path));
    }

    // =================================================================
    // Inversion & ML
    // =================================================================

    [AvaloniaFact]
    public void Capture_MLDataView()
    {
        var vm = new MLDataViewModel();
        var view = new MLDataView { DataContext = vm };
        var path = CaptureView("24_MLData", view);
        Assert.True(File.Exists(path));
    }

    [AvaloniaFact]
    public void Capture_BayesianInversionView()
    {
        var vm = new BayesianInversionViewModel();
        var view = new BayesianInversionView { DataContext = vm };
        var path = CaptureView("25_BayesianInversion", view);
        Assert.True(File.Exists(path));
    }
}
