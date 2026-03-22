using Xunit;
using ThermoElastic.Desktop.ViewModels;

namespace ThermoElastic.Desktop.E2E;

/// <summary>
/// Phase 3 E2E tests for the 6 new complex UI ViewModels.
/// Tests the full stack: ViewModel -> Calculator -> EOS -> Database.
/// </summary>
public class Phase3ViewModelE2ETests
{
    // ── IronPartitioningViewModel ──

    [Fact]
    public void Phase3_IronPartitioning_KD_LessThan1()
    {
        var vm = new IronPartitioningViewModel();
        // defaults: mpv/fpv/pe/wu, BulkXFe=0.10, P=25, T=2000
        vm.CalculateCommand.Execute(null);

        Assert.True(vm.KD > 0, "KD should be positive");
        Assert.True(vm.KD < 1, $"KD should be < 1 (Fe prefers fp), got {vm.KD}");
    }

    [Fact]
    public void Phase3_IronPartitioning_XFe_fp_GreaterThan_XFe_pv()
    {
        var vm = new IronPartitioningViewModel();
        vm.CalculateCommand.Execute(null);

        Assert.True(vm.XFeFp > vm.XFePv,
            $"XFe_fp ({vm.XFeFp}) should be > XFe_pv ({vm.XFePv})");
        Assert.Contains("Partitioning solved", vm.StatusMessage);
    }

    // ── CompositionInverterViewModel ──

    [Fact]
    public void Phase3_CompositionInverter_MisfitNonNegative()
    {
        var vm = new CompositionInverterViewModel();
        // defaults: mpv/fpv, P=38, T=2000, depth=1000, MgMin=0.5, MgMax=1.0, NSteps=20
        vm.CalculateCommand.Execute(null);

        Assert.True(vm.MinMisfit >= 0, "Misfit should be non-negative");
    }

    [Fact]
    public void Phase3_CompositionInverter_ProfileHasEntries()
    {
        var vm = new CompositionInverterViewModel();
        vm.CalculateCommand.Execute(null);

        Assert.True(vm.MisfitProfile.Count > 0, "Misfit profile should have entries");
        Assert.Contains("Best Mg#", vm.StatusMessage);
    }

    // ── EOSFitterViewModel ──

    [Fact]
    public void Phase3_EOSFitter_ConvergesOnDefaultData()
    {
        var vm = new EOSFitterViewModel();
        // default PVDataText is pre-filled with synthetic MgO BM3 data
        vm.CalculateCommand.Execute(null);

        Assert.True(vm.Converged, "Fit should converge on synthetic BM3 data");
        Assert.True(vm.FittedK0 > 100 && vm.FittedK0 < 250,
            $"Fitted K0 should be near 160 GPa, got {vm.FittedK0}");
        Assert.True(vm.FittedK1Prime > 3 && vm.FittedK1Prime < 6,
            $"Fitted K' should be near 4, got {vm.FittedK1Prime}");
    }

    // ── ElasticTensorViewModel ──

    [Fact]
    public void Phase3_ElasticTensor_ForsteriteAnisotropy_GreaterThan10Percent()
    {
        var vm = new ElasticTensorViewModel();
        vm.SelectedTensorIndex = 0; // Forsterite
        vm.DirectionX = 1; vm.DirectionY = 0; vm.DirectionZ = 0;
        vm.CalculateCommand.Execute(null);

        Assert.True(vm.MaxAnisotropy > 10,
            $"Forsterite anisotropy should be > 10%, got {vm.MaxAnisotropy}%");
    }

    [Fact]
    public void Phase3_ElasticTensor_ForsteriteVp100_InRange()
    {
        var vm = new ElasticTensorViewModel();
        vm.SelectedTensorIndex = 0; // Forsterite
        vm.DirectionX = 1; vm.DirectionY = 0; vm.DirectionZ = 0;
        vm.CalculateCommand.Execute(null);

        Assert.InRange(vm.Vp, 9000, 11000);
    }

    // ── MLDataViewModel ──

    [Fact]
    public void Phase3_MLData_Generates50Points()
    {
        var vm = new MLDataViewModel();
        vm.SelectedMineralIndex = 0; // Forsterite
        vm.NSamples = 50;
        vm.CalculateCommand.Execute(null);

        Assert.True(vm.TrainingData.Count == 50,
            $"Should generate 50 training points, got {vm.TrainingData.Count}");
    }

    [Fact]
    public void Phase3_MLData_AllVpPositive()
    {
        var vm = new MLDataViewModel();
        vm.SelectedMineralIndex = 0;
        vm.NSamples = 50;
        vm.CalculateCommand.Execute(null);

        foreach (var point in vm.TrainingData)
        {
            Assert.True(point.Vp > 0, $"All Vp should be > 0, got {point.Vp}");
        }
    }

    // ── BayesianInversionViewModel ──

    [Fact]
    public void BayesianInversionViewModel_Calculate_RecoversMean()
    {
        var vm = new BayesianInversionViewModel();
        vm.TrueMean = 3.0;
        vm.TrueSigma = 1.0;
        vm.NSamples = 5000;
        vm.BurnIn = 500;
        vm.CalculateCommand.Execute(null);
        Assert.InRange(vm.RecoveredMean, 2.5, 3.5);
        Assert.InRange(vm.AcceptanceRate, 0.1, 0.9);
        Assert.NotEmpty(vm.StatusMessage);
    }

    // ── VerificationDashboardViewModel ──

    [Fact]
    public void Phase3_VerificationDashboard_ForsteriteIsValid()
    {
        var vm = new VerificationDashboardViewModel();
        // defaults: forsterite, P=10, T=1500
        vm.CalculateCommand.Execute(null);

        Assert.True(vm.IsValid,
            $"Forsterite at 10 GPa, 1500K should pass verification. " +
            $"Maxwell={vm.MaxwellResidual:E3}, GH={vm.GibbsHelmholtzResidual:E3}, " +
            $"Entropy={vm.EntropyResidual:E3}, BulkMod={vm.BulkModulusResidual:E3}, KsKt={vm.KsKtResidual:E3}");
        Assert.Contains("PASSED", vm.StatusMessage);
    }
}
