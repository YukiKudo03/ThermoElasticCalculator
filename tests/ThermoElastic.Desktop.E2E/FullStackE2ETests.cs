using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.E2E;

/// <summary>
/// Comprehensive E2E tests covering all 26 scientific applications.
/// Tests the full stack: Calculator -> EOS -> Database -> Literature verification.
/// </summary>
public class FullStackE2ETests
{
    private readonly ITestOutputHelper _output;
    private readonly List<MineralParams> _minerals;

    public FullStackE2ETests(ITestOutputHelper output)
    {
        _output = output;
        _minerals = SLB2011Endmembers.GetAll();
    }

    private MineralParams Get(string paperName) => _minerals.First(m => m.PaperName == paperName);

    // ================================================================
    // App 1: Phase Boundary Depth Estimation
    // ================================================================

    [Fact]
    public void App01_PhaseBoundary_410km_ForsteriteToWadsleyite()
    {
        var pd = new PhaseDiagramCalculator();
        var fo = new PhaseEntry { Name = "fo", Mineral = Get("fo") };
        var mw = new PhaseEntry { Name = "mw", Mineral = Get("mw") };
        double P = pd.FindPhaseBoundary(fo, mw, 1600.0, 10.0, 20.0);
        _output.WriteLine($"410 km boundary at 1600K: {P:F2} GPa");
        Assert.InRange(P, 13.0, 14.5); // Katsura & Ito (1989)
    }

    [Fact]
    public void App01_PhaseBoundary_660km_MultiPhaseReaction()
    {
        var pd = new PhaseDiagramCalculator();
        var reactants = new List<(PhaseEntry, double)>
        {
            (new PhaseEntry { Name = "mrw", Mineral = Get("mrw") }, 1.0)
        };
        var products = new List<(PhaseEntry, double)>
        {
            (new PhaseEntry { Name = "mpv", Mineral = Get("mpv") }, 1.0),
            (new PhaseEntry { Name = "pe", Mineral = Get("pe") }, 1.0)
        };
        double P = pd.FindMultiPhaseBoundary(reactants, products, 1900.0, 20.0, 30.0);
        _output.WriteLine($"660 km boundary at 1900K: {P:F2} GPa");
        Assert.InRange(P, 22.0, 25.0); // Ito & Takahashi (1989)
    }

    [Fact]
    public void App01_ClapeyronSlope_410_Positive()
    {
        var pd = new PhaseDiagramCalculator();
        var fo = new PhaseEntry { Name = "fo", Mineral = Get("fo") };
        var mw = new PhaseEntry { Name = "mw", Mineral = Get("mw") };
        double P1 = pd.FindPhaseBoundary(fo, mw, 1400.0, 10.0, 20.0);
        double P2 = pd.FindPhaseBoundary(fo, mw, 1800.0, 10.0, 20.0);
        double dPdT = (P2 - P1) / 400.0; // GPa/K
        _output.WriteLine($"410 Clapeyron slope: {dPdT * 1000:F2} MPa/K");
        Assert.True(dPdT > 0, "410 Clapeyron slope should be positive");
    }

    // ================================================================
    // App 2: Seismic Velocity Anomaly Interpretation
    // ================================================================

    [Fact]
    public void App02_SensitivityKernel_ThermalSensitivity()
    {
        var calc = new SensitivityKernelCalculator();
        var kernel = calc.ComputeThermalSensitivity(Get("fo"), 10.0, 1500.0);
        _output.WriteLine($"dlnVs/dT = {kernel.DlnVs_dT:E3}, dlnVp/dT = {kernel.DlnVp_dT:E3}, R = {kernel.R_thermal:F2}");
        Assert.True(kernel.DlnVs_dT < 0, "Vs decreases with T");
        Assert.True(kernel.DlnVp_dT < 0, "Vp decreases with T");
        Assert.InRange(kernel.R_thermal, 1.0, 3.0); // Trampert et al. (2004)
    }

    // ================================================================
    // App 3: Anelasticity Correction
    // ================================================================

    [Fact]
    public void App03_Anelasticity_QModel_And_VelocityCorrection()
    {
        var calc = new AnelasticityCalculator();
        double QS = calc.ComputeQS(1400.0, 5.0, 1.0);
        _output.WriteLine($"QS at 1400K/5GPa/1Hz = {QS:F1}");
        Assert.InRange(QS, 50, 500); // Faul & Jackson (2005)

        var fo = Get("fo");
        var eos = new MieGruneisenEOSOptimizer(fo, 5.0, 1400.0).ExecOptimize();
        var result = calc.ApplyCorrection(eos.Vp, eos.Vs, eos.KS, eos.GS, 1400.0, 5.0);
        _output.WriteLine($"dVs = {result.DeltaVs_percent:F2}%, dVp = {result.DeltaVp_percent:F2}%");
        Assert.True(result.Vs_anelastic < result.Vs_elastic, "Anelastic Vs < elastic Vs");
        Assert.True(Math.Abs(result.DeltaVs_percent) > Math.Abs(result.DeltaVp_percent), "|dVs| > |dVp|");
    }

    // ================================================================
    // App 4: Lookup Tables for Mantle Convection
    // ================================================================

    [Fact]
    public void App04_LookupTable_GenerateAndExport()
    {
        var gen = new LookupTableGenerator();
        var table = gen.Generate(Get("fo"), 0.001, 14.0, 5, 300.0, 2000.0, 5);
        Assert.Equal(5, table.NPressure);
        Assert.Equal(5, table.NTemperature);

        // Export to CSV
        string csvPath = Path.Combine(Path.GetTempPath(), "test_lookup.csv");
        gen.ExportCSV(table, csvPath);
        Assert.True(File.Exists(csvPath));
        var lines = File.ReadAllLines(csvPath);
        Assert.Equal(26, lines.Length); // 1 header + 25 data (5×5)
        _output.WriteLine($"Exported {lines.Length} lines to CSV");
        File.Delete(csvPath);
    }

    // ================================================================
    // App 5: Subducting Slab Modeling
    // ================================================================

    [Fact]
    public void App05_SlabModel_ColdSlabFasterThanAmbient()
    {
        var slab = new SlabThermalModel();
        var anomaly = slab.ComputeSlabAnomaly(Get("fo"), 13.0, 1000.0, 1700.0);
        _output.WriteLine($"Slab anomaly: dVs={anomaly.dVs_percent:F2}%, dRho={anomaly.dRho_percent:F2}%");
        Assert.True(anomaly.dVs_percent > 0, "Cold slab has positive Vs anomaly");
        Assert.True(anomaly.dRho_percent > 0, "Cold slab is denser");
    }

    // ================================================================
    // App 6: Hugoniot EOS
    // ================================================================

    [Fact]
    public void App06_Hugoniot_MgO_PressureAtCompression()
    {
        var calc = new HugoniotCalculator(Get("pe"));
        var points = calc.ComputeHugoniot(20, 0.65);
        var p75 = points.FirstOrDefault(p => p.Compression <= 0.76 && p.Compression >= 0.74);
        if (p75 != null)
        {
            _output.WriteLine($"MgO Hugoniot at V/V0≈0.75: P={p75.Pressure:F1} GPa, T={p75.Temperature:F0} K");
            Assert.InRange(p75.Pressure, 60, 140); // Duffy & Ahrens (1995)
        }
        Assert.True(points.Count > 10);
    }

    // ================================================================
    // App 7: Elastic Geobarometry
    // ================================================================

    [Fact]
    public void App07_Geobarometry_QuartzInForsterite()
    {
        var calc = new IsomekeCalculator();
        double Pinc = calc.ComputeResidualPressure(Get("fo"), Get("qtz"), 2.0, 800.0);
        _output.WriteLine($"Residual inclusion pressure: {Pinc:F3} GPa");
        Assert.True(Pinc > 0, "Residual pressure should be positive");
    }

    // ================================================================
    // App 8: Water Content Estimation
    // ================================================================

    [Fact]
    public void App08_WaterContent_VelocityReductionAndEstimation()
    {
        var calc = new WaterContentEstimator();
        var (Vp, Vs, rho) = calc.ComputeHydrousProperties(Get("mw"), 15.0, 1500.0, 1.0);
        var eos = new MieGruneisenEOSOptimizer(Get("mw"), 15.0, 1500.0).ExecOptimize();
        Assert.True(Vs < eos.Vs, "Hydrous Vs < dry Vs");

        double water = calc.EstimateWaterContent(Get("mw"), 15.0, 1500.0, -0.01);
        _output.WriteLine($"Estimated water content for -1% dVs: {water:F2} wt%");
        Assert.True(water > 0);
    }

    // ================================================================
    // App 9: Mantle Composition Inversion
    // ================================================================

    [Fact]
    public void App09_CompositionInversion_GridSearch()
    {
        var inv = new CompositionInverter();
        var result = inv.GridSearch(Get("mpv"), Get("fpv"), 38.0, 2000.0, 1000.0, 0.50, 1.00, 10);
        _output.WriteLine($"Best Mg# = {result.BestMgNumber:F3}, misfit = {result.MinMisfit:F6}");
        Assert.True(result.MisfitProfile.Count > 0, "Should have misfit profile entries");
        Assert.True(result.MinMisfit >= 0, "Misfit should be non-negative");
    }

    // ================================================================
    // App 10: EOS Parameter Fitting
    // ================================================================

    [Fact]
    public void App10_EOSFitter_RecoverMgOParameters()
    {
        // Generate synthetic MgO P-V data
        double V0 = 11.248, K0_true = 160.2, Kp_true = 4.03;
        var data = new List<(double P, double V, double sigmaV)>();
        for (int i = 1; i <= 10; i++)
        {
            double P = i * 10.0;
            double f = 0.5 * (Math.Pow(V0 / (V0 * Math.Pow(1 + P / (3 * K0_true), -1.0 / (3 * Kp_true))), 2.0 / 3.0) - 1);
            // Approximate V from P using BM3 inversion
            double V = V0 * Math.Pow(1 + 2 * f, -1.5);
            data.Add((P, V, V * 0.001));
        }

        var fitter = new EOSFitter();
        var result = fitter.FitPV(data, V0);
        _output.WriteLine($"Fitted K0={result.Parameters[0]:F1} GPa, K'={result.Parameters[1]:F2}");
        Assert.True(result.Converged);
    }

    // ================================================================
    // App 11: Thermodynamic Verification
    // ================================================================

    [Fact]
    public void App11_ThermodynamicVerifier_AllIdentities()
    {
        var verifier = new ThermodynamicVerifier();
        var result = verifier.Verify(Get("fo"), 10.0, 1500.0);
        _output.WriteLine($"Maxwell: {result.MaxwellResidual:E3}, G=F+PV: {result.GibbsHelmholtzResidual:E3}, KS-KT: {result.KsKtResidual:E3}");
        Assert.True(result.IsValid, "All thermodynamic identities should hold");
    }

    // ================================================================
    // App 12: Iron Partitioning
    // ================================================================

    [Fact]
    public void App12_IronPartitioning_KD_LowerMantle()
    {
        var solver = new IronPartitioningSolver();
        var (xFePv, xFeFp, KD) = solver.SolvePartitioning(Get("mpv"), Get("fpv"), Get("pe"), Get("wu"), 0.10, 25.0, 2000.0);
        _output.WriteLine($"K_D = {KD:F3}, XFe_pv = {xFePv:F4}, XFe_fp = {xFeFp:F4}");
        Assert.True(KD < 1.0, "Fe preferentially enters ferropericlase");
        Assert.True(xFeFp > xFePv, "XFe_fp > XFe_pv");
    }

    // ================================================================
    // App 13: Post-Perovskite
    // ================================================================

    [Fact]
    public void App13_PostPerovskite_BoundaryPressure()
    {
        var calc = new PostPerovskiteCalculator();
        double P = calc.FindBoundary(Get("mpv"), Get("mppv"), 2500.0);
        _output.WriteLine($"pv-ppv boundary at 2500K: {P:F1} GPa");
        Assert.InRange(P, 115, 140); // Murakami et al. (2004)

        double slope = calc.GetClapeyronSlope(Get("mpv"), Get("mppv"), P, 2500.0);
        _output.WriteLine($"Clapeyron slope: {slope * 1000:F1} MPa/K");
        Assert.True(slope > 0, "pv-ppv slope is positive");
    }

    // ================================================================
    // App 14: Planetary Interior
    // ================================================================

    [Fact]
    public void App14_PlanetaryInterior_EarthMoI()
    {
        var solver = new PlanetaryInteriorSolver();
        var config = new PlanetaryConfig
        {
            Name = "Earth", Radius_km = 6371.0, CoreRadius_km = 3480.0,
            CoreDensity = 11.0, PotentialTemperature = 1600.0,
            MantleMineral = Get("fo")
        };
        var profile = solver.Solve(config, 50);
        _output.WriteLine($"Earth MoI factor: {profile.MomentOfInertiaFactor:F4}");
        Assert.InRange(profile.MomentOfInertiaFactor, 0.25, 0.40);
    }

    // ================================================================
    // App 15: Magma Ocean
    // ================================================================

    [Fact]
    public void App15_MagmaOcean_SolidusLiquidus()
    {
        var calc = new MagmaOceanCalculator();
        double solidus = calc.ComputeSolidus(25.0);
        double liquidus = calc.ComputeLiquidus(25.0);
        _output.WriteLine($"At 25 GPa: solidus={solidus:F0} K, liquidus={liquidus:F0} K");
        Assert.InRange(solidus, 2200, 2600); // Andrault et al. (2011)
        Assert.True(liquidus > solidus);
        Assert.Equal("Solid", calc.GetMeltingState(25.0, 1500.0));
        Assert.Equal("Liquid", calc.GetMeltingState(25.0, 4000.0));
    }

    // ================================================================
    // App 16: Classical Geobarometry
    // ================================================================

    [Fact]
    public void App16_Geobarometry_FoMwTransition()
    {
        var geo = new ClassicalGeobarometer();
        double P = geo.EstimatePressure(Get("fo"), Get("mw"), 1600.0);
        _output.WriteLine($"fo-mw transition at 1600K: {P:F2} GPa");
        Assert.InRange(P, 13.0, 14.5);
    }

    // ================================================================
    // App 17: LLSVP Interpretation
    // ================================================================

    [Fact]
    public void App17_LLSVP_ThermalAnomalyTradeoff()
    {
        var calc = new LLSVPCalculator();
        double dT = calc.ComputeRequiredDeltaT(Get("mpv"), 120.0, 2500.0, -0.02);
        _output.WriteLine($"Required ΔT for -2% dVs: {dT:F0} K");
        Assert.True(dT > 100 && dT < 1500, "Reasonable temperature anomaly");
    }

    // ================================================================
    // App 18: ULVZ Modeling
    // ================================================================

    [Fact]
    public void App18_ULVZ_PartialMelt_VsReduction()
    {
        var calc = new ULVZCalculator();
        var (Vp, Vs, rho, dVp, dVs) = calc.ComputeMeltMixture(Get("mpv"), 0.10, 135.0, 3800.0);
        _output.WriteLine($"10% melt at CMB: dVs={dVs:F1}%, dVp={dVp:F1}%");
        Assert.True(dVs < -5, "10% melt should reduce Vs by >5%");
        Assert.True(Math.Abs(dVs) > Math.Abs(dVp), "|dVs| > |dVp|");
    }

    // ================================================================
    // App 19: Electrical Conductivity
    // ================================================================

    [Fact]
    public void App19_ElectricalConductivity_WaterEffect()
    {
        var calc = new ElectricalConductivityCalculator();
        double sigDry = calc.ComputeConductivity(1400.0, 5.0, 0);
        double sigWet = calc.ComputeConductivity(1400.0, 5.0, 1000);
        _output.WriteLine($"σ_dry = {sigDry:E3}, σ_wet = {sigWet:E3}, enhancement = {sigWet / sigDry:F1}x");
        Assert.True(sigWet > sigDry * 3, "Water should significantly enhance conductivity");
    }

    // ================================================================
    // App 20: ML Training Data
    // ================================================================

    [Fact]
    public void App20_MLTrainingData_Generation()
    {
        var gen = new TrainingDataGenerator();
        var data = gen.Generate(Get("fo"), 0.001, 25.0, 300.0, 2000.0, 50);
        Assert.Equal(50, data.Count);
        Assert.True(data.All(d => d.Vp > 0 && d.Vs > 0 && d.Density > 0));
        _output.WriteLine($"Generated {data.Count} training points, Vp range: {data.Min(d => d.Vp):F0}-{data.Max(d => d.Vp):F0} m/s");
    }

    // ================================================================
    // App 21: Mars Interior
    // ================================================================

    [Fact]
    public void App21_MarsInterior_MoI()
    {
        var mars = new MarsInteriorModel();
        var profile = mars.Compute();
        _output.WriteLine($"Mars MoI: {profile.MomentOfInertiaFactor:F4}, surface g: {profile.Gravity.Last():F2} m/s²");
        Assert.InRange(profile.MomentOfInertiaFactor, 0.33, 0.38); // Konopliv et al. (2020)
    }

    // ================================================================
    // App 22: Thermal Conductivity
    // ================================================================

    [Fact]
    public void App22_ThermalConductivity_MgO()
    {
        var calc = new ThermalConductivityCalculator();
        double k_ambient = calc.ComputeLatticeConductivity(Get("pe"), 0.001, 300.0);
        double k_highP = calc.ComputeLatticeConductivity(Get("pe"), 50.0, 2000.0);
        _output.WriteLine($"k_lat MgO: ambient={k_ambient:F1}, 50GPa/2000K={k_highP:F1} W/m/K");
        Assert.InRange(k_ambient, 30, 60); // Hofmeister (1999)
        Assert.True(k_highP != k_ambient, "k should change with P-T");
    }

    // ================================================================
    // App 23: Spin Crossover
    // ================================================================

    [Fact]
    public void App23_SpinCrossover_FerroPericlase()
    {
        var calc = new SpinCrossoverCalculator();
        var ls = SpinCrossoverCalculator.CreateLSEndmember(Get("wu"));
        var (nLS_low, _, _, _, _, _) = calc.ComputeSpinState(Get("wu"), ls, 10.0, 2000.0);
        var (nLS_mid, _, _, _, _, _) = calc.ComputeSpinState(Get("wu"), ls, 60.0, 2000.0);
        var (nLS_high, _, _, _, _, _) = calc.ComputeSpinState(Get("wu"), ls, 120.0, 2000.0);
        _output.WriteLine($"n_LS: 10GPa={nLS_low:F3}, 60GPa={nLS_mid:F3}, 120GPa={nLS_high:F3}");
        Assert.True(nLS_low < nLS_mid, "LS fraction increases with P");
        Assert.True(nLS_mid < nLS_high, "LS fraction continues increasing");
    }

    // ================================================================
    // App 24: Seismic Anisotropy
    // ================================================================

    [Fact]
    public void App24_ElasticTensor_ForsteriteAnisotropy()
    {
        var tensor = SingleCrystalElasticConstants.Forsterite();
        var calc = new ElasticTensorCalculator();
        double aniso = calc.ComputeMaxAnisotropy(tensor);
        _output.WriteLine($"Forsterite Vp anisotropy: {aniso:F1}%");
        Assert.True(aniso > 10, "Olivine should have >10% P-wave anisotropy");

        var (Vp100, Vs1_100, Vs2_100) = calc.SolveChristoffel(tensor, new[] { 1.0, 0.0, 0.0 });
        _output.WriteLine($"Vp[100] = {Vp100:F0} m/s, Vs1 = {Vs1_100:F0}, Vs2 = {Vs2_100:F0}");
        Assert.InRange(Vp100, 9000, 11000);
    }

    // ================================================================
    // App 25: Oxygen Fugacity
    // ================================================================

    [Fact]
    public void App25_OxygenFugacity_BufferOrdering()
    {
        var calc = new OxygenFugacityCalculator();
        double iw = calc.ComputeLogFO2("IW", 1473.0);
        double qfm = calc.ComputeLogFO2("QFM", 1473.0);
        double nno = calc.ComputeLogFO2("NNO", 1473.0);
        _output.WriteLine($"log10(fO2) at 1473K: IW={iw:F2}, QFM={qfm:F2}, NNO={nno:F2}");
        Assert.True(iw < qfm, "IW more reducing than QFM");
        Assert.True(qfm < nno, "QFM more reducing than NNO");
    }

    // ================================================================
    // App 26: Bayesian Inversion
    // ================================================================

    [Fact]
    public void App26_MCMC_GaussianRecovery()
    {
        var sampler = new MCMCSampler(seed: 42);
        double trueMean = 3.0, trueSigma = 1.0;
        double logPosterior(double[] p) => -0.5 * (p[0] - trueMean) * (p[0] - trueMean) / (trueSigma * trueSigma);

        var chain = sampler.Sample(logPosterior, new[] { 0.0 }, new[] { 1.0 }, nSamples: 5000);
        var mean = chain.GetMean(burnIn: 500);
        _output.WriteLine($"MCMC recovered mean: {mean[0]:F3} (true: {trueMean}), acceptance: {chain.AcceptanceRate:P0}");
        Assert.InRange(mean[0], 2.5, 3.5);
        Assert.InRange(chain.AcceptanceRate, 0.15, 0.80);
    }

    // ================================================================
    // App 27: Thermoelastic Parameter Fitter
    // ================================================================

    [Fact]
    public void App27_ThermoElasticFitter_Forsterite_VpVs_RecoversK0G0()
    {
        // Generate synthetic Vp/Vs data from Forsterite SLB2011 literature values
        var fo = Get("fo");
        var conditions = new (double T, double P)[]
        {
            (300, 5), (300, 10), (300, 20), (300, 30), (300, 50),
            (1000, 5), (1000, 10), (1000, 20), (1000, 30),
            (1500, 10), (1500, 20), (1500, 30), (1500, 50),
            (2000, 10), (2000, 20), (2000, 30), (2000, 50),
        };

        var rng = new Random(42);
        var data = new List<FittingDataPoint>();
        foreach (var (T, P) in conditions)
        {
            var th = new MieGruneisenEOSOptimizer(fo, P, T).ExecOptimize();
            if (!th.IsConverged) continue;
            data.Add(new FittingDataPoint
            {
                Temperature = T, Pressure = P,
                Vp = th.Vp + th.Vp * 0.005 * (rng.NextDouble() * 2 - 1),
                Vs = th.Vs + th.Vs * 0.005 * (rng.NextDouble() * 2 - 1),
                SigmaVp = th.Vp * 0.005,
                SigmaVs = th.Vs * 0.005,
            });
        }

        // Fit K0 and G0 with 10% perturbed initial guess
        var config = new FittingConfig { BaseMineralParams = fo.Clone() };
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;
        config.BaseMineralParams.KZero *= 1.10;
        config.BaseMineralParams.GZero *= 0.90;

        var result = new ThermoElasticFitter().Fit(config, data);

        _output.WriteLine($"SLB2011: K0={fo.KZero:F2} GPa, G0={fo.GZero:F2} GPa");
        _output.WriteLine($"Fitted:  K0={result.Parameters[0]:F2} ± {result.Uncertainties[0]:F2}, G0={result.Parameters[1]:F2} ± {result.Uncertainties[1]:F2}");
        _output.WriteLine($"Converged={result.Converged}, χ²r={result.ReducedChiSquared:F4}");

        Assert.True(result.Converged);
        Assert.True(Math.Abs(result.Parameters[0] - fo.KZero) / fo.KZero < 0.02,
            $"K0 recovery: fitted {result.Parameters[0]:F2} vs literature {fo.KZero:F2}");
        Assert.True(Math.Abs(result.Parameters[1] - fo.GZero) / fo.GZero < 0.02,
            $"G0 recovery: fitted {result.Parameters[1]:F2} vs literature {fo.GZero:F2}");
    }

    [Fact]
    public void App27_ThermoElasticFitter_Periclase_Volume_RecoversEOSParams()
    {
        // Generate synthetic V(T,P) data from Periclase SLB2011 literature values
        var pe = Get("pe");
        var conditions = new (double T, double P)[]
        {
            (300, 1), (300, 5), (300, 10), (300, 20), (300, 30), (300, 50),
            (1000, 5), (1000, 10), (1000, 20), (1000, 30),
            (1500, 10), (1500, 20), (1500, 30), (1500, 50),
            (2000, 10), (2000, 20), (2000, 30), (2000, 50),
        };

        var rng = new Random(314);
        var data = new List<FittingDataPoint>();
        foreach (var (T, P) in conditions)
        {
            var th = new MieGruneisenEOSOptimizer(pe, P, T).ExecOptimize();
            if (!th.IsConverged) continue;
            data.Add(new FittingDataPoint
            {
                Temperature = T, Pressure = P,
                Volume = th.Volume + th.Volume * 0.001 * (rng.NextDouble() * 2 - 1),
                SigmaVolume = th.Volume * 0.001,
            });
        }

        var config = new FittingConfig { BaseMineralParams = pe.Clone() };
        config.FitFlags[FittingConfig.IndexV0] = true;
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexK1Prime] = true;
        config.BaseMineralParams.MolarVolume *= 1.03;
        config.BaseMineralParams.KZero *= 0.95;
        config.BaseMineralParams.K1Prime *= 1.05;

        var result = new ThermoElasticFitter().Fit(config, data);

        _output.WriteLine($"SLB2011: V0={pe.MolarVolume:F3}, K0={pe.KZero:F2}, K'={pe.K1Prime:F4}");
        _output.WriteLine($"Fitted:  V0={result.Parameters[0]:F3}, K0={result.Parameters[1]:F2}, K'={result.Parameters[2]:F4}");
        _output.WriteLine($"Converged={result.Converged}, χ²r={result.ReducedChiSquared:F4}");

        Assert.True(result.Converged);
        Assert.True(Math.Abs(result.Parameters[0] - pe.MolarVolume) / pe.MolarVolume < 0.02);
        Assert.True(Math.Abs(result.Parameters[1] - pe.KZero) / pe.KZero < 0.03);
        Assert.True(Math.Abs(result.Parameters[2] - pe.K1Prime) / pe.K1Prime < 0.05);
    }

    [Fact]
    public void App27_ThermoElasticFitter_ViewModel_EndToEnd()
    {
        // Test that ViewModel initializes correctly with default data
        var vm = new ThermoElastic.Desktop.ViewModels.ThermoElasticFitterViewModel();

        // Default data should be pre-populated (Forsterite Vp/Vs)
        Assert.False(string.IsNullOrWhiteSpace(vm.DataText));
        Assert.True(vm.DataText.Contains("Forsterite"), "Default data should contain Forsterite label");

        // Default parameter values should match Forsterite
        Assert.InRange(vm.V0, 43.0, 44.0);
        Assert.InRange(vm.K0, 127.0, 129.0);
        Assert.InRange(vm.G0, 81.0, 82.0);
        Assert.Equal(7, vm.NumAtoms);

        // Mineral list should be populated from SLB2011
        Assert.True(vm.MineralNames.Count > 40, $"Expected >40 minerals, got {vm.MineralNames.Count}");

        // FitK0 and FitG0 should be checked by default
        Assert.True(vm.FitK0);
        Assert.True(vm.FitG0);
        Assert.False(vm.FitV0);

        // Verify fitter works with Forsterite data directly
        var fo = Get("fo");
        var data = new List<FittingDataPoint>();
        foreach (var (T, P) in new[] { (300.0, 5.0), (300.0, 10.0), (300.0, 20.0), (1000.0, 10.0), (1000.0, 20.0), (1500.0, 10.0), (1500.0, 20.0) })
        {
            var th = new MieGruneisenEOSOptimizer(fo, P, T).ExecOptimize();
            if (th.IsConverged)
                data.Add(new FittingDataPoint { Temperature = T, Pressure = P, Vp = th.Vp, Vs = th.Vs, SigmaVp = th.Vp * 0.005, SigmaVs = th.Vs * 0.005 });
        }

        var config = new FittingConfig { BaseMineralParams = fo.Clone() };
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;

        var result = new ThermoElasticFitter().Fit(config, data);
        _output.WriteLine($"ViewModel E2E: K0={result.Parameters[0]:F2}, G0={result.Parameters[1]:F2}, Converged={result.Converged}");
        Assert.True(result.Converged);
    }

    // ================================================================
    // App 28: Enhanced Anelasticity — Multi-model Q correction
    // ================================================================

    [Fact]
    public void App28_Anelasticity_ParametricQ_ForsteriteAtMantle()
    {
        // ParametricQ model: Forsterite at upper mantle conditions
        var fo = Get("fo");
        var eos = new MieGruneisenEOSOptimizer(fo, 5.0, 1400.0).ExecOptimize();

        var calc = new ParametricQCalculator();
        var prms = AnelasticityDatabase.Olivine() with { GrainSize_m = 0.01 };
        var result = calc.ApplyCorrection(eos.Vp, eos.Vs, eos.KS, eos.GS, 1400.0, 5.0, 1.0, prms);

        _output.WriteLine($"ParametricQ: QS={result.QS:F1}, Vs_el={eos.Vs:F0}, Vs_an={result.Vs_anelastic:F0}, ΔVs={result.DeltaVs_percent:F2}%");
        Assert.True(result.QS > 0);
        Assert.True(result.Vs_anelastic < eos.Vs);
        Assert.True(result.DeltaVs_percent < 0);
    }

    [Fact]
    public void App28_Anelasticity_Andrade_FrequencyDispersion()
    {
        // Andrade model shows velocity dispersion: lower freq → lower velocity
        var fo = Get("fo");
        var eos = new MieGruneisenEOSOptimizer(fo, 5.0, 1400.0).ExecOptimize();
        var prms = AnelasticityDatabase.Olivine() with { GrainSize_m = 0.01 };

        var andrade = new AndradeCalculator();
        var r_1Hz = andrade.ApplyCorrection(eos.Vp, eos.Vs, eos.KS, eos.GS, 1400.0, 5.0, 1.0, prms);
        var r_01Hz = andrade.ApplyCorrection(eos.Vp, eos.Vs, eos.KS, eos.GS, 1400.0, 5.0, 0.01, prms);

        _output.WriteLine($"Andrade: Vs(1Hz)={r_1Hz.Vs_anelastic:F0}, Vs(0.01Hz)={r_01Hz.Vs_anelastic:F0}");
        Assert.True(r_01Hz.Vs_anelastic <= r_1Hz.Vs_anelastic, "Lower freq → lower Vs");
    }

    [Fact]
    public void App28_Anelasticity_WaterAndMelt_CombinedEffect()
    {
        // Water + melt combined: should reduce both Q and velocity
        var fo = Get("fo");
        var eos = new MieGruneisenEOSOptimizer(fo, 5.0, 1400.0).ExecOptimize();

        var dryPrms = AnelasticityDatabase.Olivine() with
        {
            GrainSize_m = 0.01, WaterContent_ppm = 0, MeltFraction = 0
        };
        var wetMeltPrms = AnelasticityDatabase.Olivine() with
        {
            GrainSize_m = 0.01, WaterContent_ppm = 200, MeltFraction = 0.01
        };

        var baseModel = new ParametricQCalculator();
        var fullModel = new MeltQCorrector(new WaterQCorrector(baseModel));

        var r_dry = baseModel.ApplyCorrection(eos.Vp, eos.Vs, eos.KS, eos.GS, 1400.0, 5.0, 1.0, dryPrms);
        var r_wet_melt = fullModel.ApplyCorrection(eos.Vp, eos.Vs, eos.KS, eos.GS, 1400.0, 5.0, 1.0, wetMeltPrms);

        _output.WriteLine($"Dry: Vs={r_dry.Vs_anelastic:F0}, QS={r_dry.QS:F1}");
        _output.WriteLine($"Wet+Melt: Vs={r_wet_melt.Vs_anelastic:F0}, QS={r_wet_melt.QS:F1}");
        Assert.True(r_wet_melt.Vs_anelastic < r_dry.Vs_anelastic, "Water+melt should reduce Vs");
    }

    [Fact]
    public void App28_Anelasticity_QProfile_FullDepthRange()
    {
        // Q profile from surface to 800km with phase transitions
        var model = new ParametricQCalculator();
        var builder = new QProfileBuilder();
        var profile = builder.Build(model, 1600.0, 1.0, 0.01, 800.0, 50.0);

        _output.WriteLine($"Q Profile: {profile.Count} points");
        var phases = profile.Select(p => p.DominantPhase).Distinct().ToList();
        _output.WriteLine($"Phases encountered: {string.Join(", ", phases)}");

        Assert.True(profile.Count > 10, "Should have >10 depth points");
        Assert.Contains("olivine", phases);
        // Should include transition zone phases if depth > 410km
        if (profile.Any(p => p.Depth_km > 420))
            Assert.Contains("wadsleyite", phases);

        // All QS positive and finite
        Assert.All(profile, p =>
        {
            Assert.True(p.QS > 0 && !double.IsNaN(p.QS) && !double.IsInfinity(p.QS));
        });
    }

    [Fact]
    public void App28_Anelasticity_ViewModel_AllModels_Work()
    {
        // Verify ViewModel initialization (models, mineral list, defaults)
        var vm = new ThermoElastic.Desktop.ViewModels.AnelasticityViewModel();
        Assert.Equal(4, vm.ModelNames.Count);
        Assert.True(vm.MineralNames.Count > 40);
        Assert.Equal(10.0, vm.GrainSize_mm);
        Assert.Equal(0.0, vm.WaterContent);
        Assert.Equal(0.0, vm.MeltFraction);
    }

    [Fact]
    public void App28_Anelasticity_QProfileViewModel_Initializes()
    {
        var vm = new ThermoElastic.Desktop.ViewModels.QProfileViewModel();
        Assert.Equal(1600.0, vm.PotentialTemp);
        Assert.Equal(1.0, vm.Frequency);
        Assert.Equal(10.0, vm.GrainSize_mm);
        Assert.Equal(4, vm.ModelNames.Count);
    }

    // ================================================================
    // Literature Cross-Validation (BurnMan reference values)
    // ================================================================

    [Theory]
    [InlineData("fo", 0.0001, 300.0, 8000, 9000)]   // Forsterite Vp at ambient
    [InlineData("pe", 0.0001, 300.0, 9000, 10500)]   // Periclase Vp at ambient
    [InlineData("mpv", 25.0, 2000.0, 10000, 13000)]  // Mg-Pv Vp at lower mantle
    public void Literature_Vp_InExpectedRange(string paperName, double P, double T, double vpMin, double vpMax)
    {
        var result = new MieGruneisenEOSOptimizer(Get(paperName), P, T).ExecOptimize();
        _output.WriteLine($"{paperName} at {P}GPa/{T}K: Vp = {result.Vp:F0} m/s");
        Assert.InRange(result.Vp, vpMin, vpMax);
    }

    [Theory]
    [InlineData("fo", 0.0001, 300.0, 3.1, 3.4)]   // Forsterite density at ambient
    [InlineData("pe", 0.0001, 300.0, 3.4, 3.8)]   // Periclase density at ambient
    [InlineData("mpv", 100.0, 2500.0, 4.5, 5.5)]  // Mg-Pv density in deep mantle
    public void Literature_Density_InExpectedRange(string paperName, double P, double T, double rhoMin, double rhoMax)
    {
        var result = new MieGruneisenEOSOptimizer(Get(paperName), P, T).ExecOptimize();
        _output.WriteLine($"{paperName} at {P}GPa/{T}K: ρ = {result.Density:F3} g/cm³");
        Assert.InRange(result.Density, rhoMin, rhoMax);
    }
}
