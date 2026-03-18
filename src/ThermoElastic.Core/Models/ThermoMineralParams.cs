using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Core.Models;

public class ThermoMineralParams
{
    public ThermoMineralParams(double targetFinite, double targetTemperature, MineralParams mineral)
    {
        _targetTemperature = targetTemperature;
        _mineral = mineral;
        _targetFinite = targetFinite;
        _refP = _mineral.GetPressure(_targetFinite);
        _mu = 1.0d + _mineral.Aii * _targetFinite + _mineral.Aiikk * _targetFinite * _targetFinite / 2.0d;
        _gamma = 1.0d / _mu * (2.0d * _targetFinite + 1.0d) * (_mineral.Aii + _mineral.Aiikk * _targetFinite) / 6.0d;
        _ethaS = -Gamma - (2.0d * Finite + 1.0d) * (2.0d * Finite + 1.0d) * Mineral.As / _mu / 2.0d;
        _vibrationalDebyeTemp = Math.Sqrt(_mu) * _mineral.DebyeTempZero;
        _debyeCondition = new DebyeFunctionCalculator(_vibrationalDebyeTemp);
        _deltaP = (Gamma / Volume) * DeltaE / 1000.0d;
        _q = 1.0d / 9.0d * (18.0d * Gamma * Gamma - 6.0 * Gamma - 1.0d / 2.0d / _mu * (2.0d * Finite + 1.0d) * (2.0d * Finite + 1.0d) * Mineral.Aiikk) / Gamma;

        // Landau phase transition: compute Tc at current pressure, then corrections
        _landauTc = LandauCalculator.GetTc(Pressure, mineral.Tc0, mineral.VD, mineral.SD);
        _landauVolume = LandauCalculator.GetVolume(targetTemperature, _landauTc, mineral.VD);
        _landauEntropy = LandauCalculator.GetEntropy(targetTemperature, _landauTc, mineral.SD);
        _landauFreeEnergy = LandauCalculator.GetFreeEnergy(targetTemperature, _landauTc, mineral.SD);

        // Magnetic contribution: F_mag = -T * r * R * ln(2S+1)
        if (mineral.MagneticAtomCount > 0 && mineral.SpinQuantumNumber > 0)
        {
            _magneticFreeEnergy = -targetTemperature * mineral.MagneticAtomCount
                * PhysicConstants.GasConst * Math.Log(2.0 * mineral.SpinQuantumNumber + 1.0);
            _magneticEntropy = mineral.MagneticAtomCount
                * PhysicConstants.GasConst * Math.Log(2.0 * mineral.SpinQuantumNumber + 1.0);
        }
    }

    private readonly double _mu;
    private readonly double _deltaP;
    private readonly double _refP;
    private readonly MineralParams _mineral;
    private readonly DebyeFunctionCalculator _debyeCondition;
    private readonly double _targetFinite;
    private readonly double _targetTemperature;
    private readonly double _vibrationalDebyeTemp;
    private readonly double _gamma;
    private readonly double _ethaS;
    private readonly double _q;

    // Landau phase transition fields
    private readonly double _landauTc;
    private readonly double _landauVolume;
    private readonly double _landauEntropy;
    private readonly double _landauFreeEnergy;

    // Magnetic contribution fields
    private readonly double _magneticFreeEnergy;
    private readonly double _magneticEntropy;

    /// <summary>Temperature Effect on Pressure [GPa]</summary>
    public double DeltaP => _deltaP;

    /// <summary>Pressure without Temperature Effect [GPa]</summary>
    public double RefP => _refP;

    /// <summary>Sum of Pressure with and without Temperature Effect [GPa]</summary>
    public double Pressure => _refP + _deltaP;

    public MineralParams Mineral => _mineral;

    /// <summary>Finite Strain</summary>
    public double Finite => _targetFinite;

    /// <summary>Molar Volume under Condition [cm3/mol] (includes Landau correction)</summary>
    public double Volume => _mineral.FiniteToVolume(Finite) + _landauVolume;

    /// <summary>Landau critical temperature at current pressure [K]</summary>
    public double LandauTc => _landauTc;
    /// <summary>Landau free energy contribution [J/mol]</summary>
    public double LandauFreeEnergy => _landauFreeEnergy;
    /// <summary>Landau entropy contribution [J/mol/K]</summary>
    public double LandauEntropy => _landauEntropy;
    /// <summary>Landau volume contribution [cm³/mol]</summary>
    public double LandauVolume => _landauVolume;

    /// <summary>Magnetic free energy contribution [J/mol]</summary>
    public double MagneticFreeEnergy => _magneticFreeEnergy;
    /// <summary>Magnetic entropy contribution [J/mol/K]</summary>
    public double MagneticEntropy => _magneticEntropy;

    /// <summary>Calculate Temperature [K]</summary>
    public double Temperature => _targetTemperature;

    /// <summary>Debye Temperature under Finite [K]</summary>
    public double DebyeTemperature => _vibrationalDebyeTemp;

    public double DeltaE =>
        (_debyeCondition.GetInternalEnergy(Temperature) - _debyeCondition.GetInternalEnergy(_mineral.RefTemp)) * _mineral.NumAtoms;

    public double CvT =>
        _debyeCondition.GetCv(Temperature) * _mineral.NumAtoms * Temperature;

    public double DeltaCvT =>
        CvT - _debyeCondition.GetCv(_mineral.RefTemp) * _mineral.NumAtoms * _mineral.RefTemp;

    /// <summary>Gruneisen Parameter under Condition</summary>
    public double Gamma => _gamma;

    /// <summary>Density under Condition [g/cm3]</summary>
    public double Density => _mineral.MolarWeight / Volume;

    /// <summary>Isothermal Bulk Modulus under Condition [GPa]</summary>
    public double KT
    {
        get
        {
            var a = (Gamma + 1.0d - Mineral.QZero) * Gamma * DeltaE / Volume / 1000.0d;
            var b = Gamma * Gamma * DeltaCvT / Volume / 1000.0d;
            return Mineral.BM3KT(Finite) + a - b;
        }
    }

    /// <summary>Thermal Expansion under Condition [1/K]</summary>
    public double Alpha => Gamma * CvT / Temperature / KT / Volume / 1000.0d;

    /// <summary>Adiabatic Bulk Modulus under Condition [GPa]</summary>
    public double KS
    {
        get
        {
            var a = Gamma * Gamma / Volume * CvT / 1000.0d;
            return KT + a;
        }
    }

    public double EthaS => _ethaS;

    public double Q => _q;

    /// <summary>Shear Modulus under Condition [GPa]</summary>
    public double GS
    {
        get
        {
            var b = EthaS / Volume * DeltaE / 1000.0d;
            return Mineral.BM3GT(Finite) - b;
        }
    }

    /// <summary>Primary wave velocity [m/s]</summary>
    public double Vp => 1000.0d * Math.Sqrt((KS + 4.0d / 3.0d * GS) / Density);

    /// <summary>Secondary wave velocity [m/s]</summary>
    public double Vs => 1000.0d * Math.Sqrt(GS / Density);

    /// <summary>Bulk Sound Velocity [m/s]</summary>
    public double Vb => 1000.0d * Math.Sqrt(KS / Density);

    /// <summary>
    /// Cold (compression) energy: F_cold = 9*K0*V0 * [f²/2 + a1*f³/6]
    /// where a1 = K'0 - 4. Result in kJ/mol (V0 in cm³/mol, K0 in GPa).
    /// 1 GPa·cm³/mol = 1 kJ/mol, so 9*K0*V0*(strain) is directly in kJ/mol.
    /// </summary>
    public double FCold
    {
        get
        {
            double f = Finite;
            double a1 = Mineral.K1Prime - 4.0;
            return 9.0 * Mineral.KZero * Mineral.MolarVolume * (f * f / 2.0 + a1 * f * f * f / 6.0);
        }
    }

    /// <summary>
    /// Thermal free energy: n*kB*T * [3*ln(1-e^(-θ/T)) - D3(θ/T)]
    /// minus the reference at T_ref. Result in kJ/mol.
    /// </summary>
    public double FThermal
    {
        get
        {
            var refDebye = new DebyeFunctionCalculator(Mineral.DebyeTempZero);
            double fAtT = _debyeCondition.GetThermalFreeEnergyPerAtom(Temperature);
            double fAtRef = refDebye.GetThermalFreeEnergyPerAtom(Mineral.RefTemp);
            // kB*T per atom, n atoms, convert J→kJ
            double kB = PhysicConstants.Boltzman;
            return Mineral.NumAtoms * (kB * Temperature * fAtT - kB * Mineral.RefTemp * fAtRef)
                * PhysicConstants.NA / 1000.0;
        }
    }

    /// <summary>
    /// Total Helmholtz free energy F = F0 + F_cold + F_thermal + F_Landau + F_mag [kJ/mol]
    /// </summary>
    public double HelmholtzF =>
        Mineral.F0 + FCold + FThermal + LandauFreeEnergy / 1000.0 + MagneticFreeEnergy / 1000.0;

    /// <summary>
    /// Entropy S = -∂F/∂T (numerical central difference) [J/mol/K]
    /// </summary>
    public double Entropy
    {
        get
        {
            double dT = 0.5;
            double T = Temperature;
            if (T < dT + 1) return 0.0;

            // Compute F at T+dT and T-dT using same finite strain
            var thPlus = new ThermoMineralParams(Finite, T + dT, Mineral);
            var thMinus = new ThermoMineralParams(Finite, T - dT, Mineral);
            return -(thPlus.HelmholtzF - thMinus.HelmholtzF) / (2.0 * dT) * 1000.0; // kJ→J
        }
    }

    /// <summary>
    /// Gibbs free energy G = F + PV [kJ/mol]
    /// P in GPa, V in cm³/mol → PV in GPa·cm³/mol = kJ/mol (1 GPa·cm³ = 1 kJ).
    /// </summary>
    public double GibbsG => HelmholtzF + Pressure * Volume;

    public ResultSummary ExportResults()
    {
        return new ResultSummary
        {
            Name = Mineral.MineralName,
            GivenP = Pressure,
            GivenT = Temperature,
            Alpha = Alpha,
            DebyeTemp = DebyeTemperature,
            Density = Density,
            EthaS = EthaS,
            Gamma = Gamma,
            GS = GS,
            KS = KS,
            KT = KT,
            Q = Q,
            Volume = Volume,
            HelmholtzF = HelmholtzF,
            GibbsG = GibbsG,
            Entropy = Entropy,
        };
    }
}
