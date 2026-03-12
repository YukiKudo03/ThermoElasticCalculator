using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Database;

/// <summary>
/// SLB2011 Table A1 endmember mineral parameters.
/// Stixrude &amp; Lithgow-Bertelloni (2011) Geophys. J. Int. 184, 1180-1213.
/// Units: V0 [cm³/mol], K0 [GPa], G0 [GPa], θ0 [K], F0 [kJ/mol]
/// Landau: Tc0 [K], VD [cm³/mol], SD [J/mol/K]
/// Magnetic: S = spin quantum number, r = magnetic atoms per f.u.
/// </summary>
public static class SLB2011Endmembers
{
    public static List<MineralParams> GetAll()
    {
        return new List<MineralParams>
        {
            // ==================== Olivine system ====================
            new MineralParams
            {
                MineralName = "Forsterite", PaperName = "fo",
                NumAtoms = 7, MolarVolume = 43.603, MolarWeight = 140.69,
                KZero = 128.0, K1Prime = 4.2, GZero = 82.0, G1Prime = 1.5,
                DebyeTempZero = 809.0, GammaZero = 0.99, QZero = 2.1, EhtaZero = 2.3,
                F0 = -2055.403, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fayalite", PaperName = "fa",
                NumAtoms = 7, MolarVolume = 46.39, MolarWeight = 203.77,
                KZero = 135.0, K1Prime = 4.2, GZero = 51.0, G1Prime = 1.5,
                DebyeTempZero = 619.0, GammaZero = 1.06, QZero = 3.6, EhtaZero = 1.0,
                F0 = -1371.588, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 2.0,
            },

            // ==================== Wadsleyite system ====================
            new MineralParams
            {
                MineralName = "Mg-Wadsleyite", PaperName = "mw",
                NumAtoms = 28, MolarVolume = 170.84, MolarWeight = 562.76,
                KZero = 169.0, K1Prime = 4.3, GZero = 112.0, G1Prime = 1.5,
                DebyeTempZero = 844.0, GammaZero = 1.21, QZero = 2.0, EhtaZero = 2.6,
                F0 = -8222.789, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fe-Wadsleyite", PaperName = "fw",
                NumAtoms = 28, MolarVolume = 183.12, MolarWeight = 815.08,
                KZero = 169.0, K1Prime = 4.3, GZero = 72.0, G1Prime = 1.5,
                DebyeTempZero = 637.0, GammaZero = 1.21, QZero = 2.0, EhtaZero = 2.6,
                F0 = -5489.081, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 8.0,
            },

            // ==================== Ringwoodite system ====================
            new MineralParams
            {
                MineralName = "Mg-Ringwoodite", PaperName = "mrw",
                NumAtoms = 7, MolarVolume = 39.49, MolarWeight = 140.69,
                KZero = 185.0, K1Prime = 4.2, GZero = 120.0, G1Prime = 1.4,
                DebyeTempZero = 878.0, GammaZero = 1.11, QZero = 2.4, EhtaZero = 2.6,
                F0 = -2017.996, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fe-Ringwoodite", PaperName = "frw",
                NumAtoms = 7, MolarVolume = 42.03, MolarWeight = 203.77,
                KZero = 213.0, K1Prime = 4.2, GZero = 92.0, G1Prime = 1.4,
                DebyeTempZero = 679.0, GammaZero = 1.19, QZero = 2.4, EhtaZero = 2.6,
                F0 = -1361.649, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 2.0,
            },

            // ==================== Perovskite (Bridgmanite) system ====================
            new MineralParams
            {
                MineralName = "Mg-Perovskite", PaperName = "mpv",
                NumAtoms = 5, MolarVolume = 24.45, MolarWeight = 100.39,
                KZero = 251.0, K1Prime = 4.1, GZero = 173.0, G1Prime = 1.7,
                DebyeTempZero = 905.0, GammaZero = 1.44, QZero = 1.4, EhtaZero = 2.6,
                F0 = -1368.476, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fe-Perovskite", PaperName = "fpv",
                NumAtoms = 5, MolarVolume = 25.49, MolarWeight = 131.93,
                KZero = 272.0, K1Prime = 4.1, GZero = 133.0, G1Prime = 1.4,
                DebyeTempZero = 871.0, GammaZero = 1.57, QZero = 1.1, EhtaZero = 2.3,
                F0 = -1003.505, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },
            new MineralParams
            {
                MineralName = "Al-Perovskite", PaperName = "apv",
                NumAtoms = 5, MolarVolume = 24.77, MolarWeight = 101.96,
                KZero = 232.0, K1Prime = 4.3, GZero = 165.0, G1Prime = 1.8,
                DebyeTempZero = 886.0, GammaZero = 1.54, QZero = 1.0, EhtaZero = 1.3,
                F0 = -1540.800, RefTemp = 300.0,
            },

            // ==================== Post-Perovskite ====================
            new MineralParams
            {
                MineralName = "Mg-PostPerovskite", PaperName = "mppv",
                NumAtoms = 5, MolarVolume = 24.03, MolarWeight = 100.39,
                KZero = 231.0, K1Prime = 4.0, GZero = 150.0, G1Prime = 2.0,
                DebyeTempZero = 855.0, GammaZero = 1.48, QZero = 1.1, EhtaZero = 3.0,
                F0 = -1348.570, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fe-PostPerovskite", PaperName = "fppv",
                NumAtoms = 5, MolarVolume = 25.05, MolarWeight = 131.93,
                KZero = 250.0, K1Prime = 4.0, GZero = 123.0, G1Prime = 1.7,
                DebyeTempZero = 758.0, GammaZero = 1.48, QZero = 1.1, EhtaZero = 3.0,
                F0 = -990.780, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },
            new MineralParams
            {
                MineralName = "Al-PostPerovskite", PaperName = "appv",
                NumAtoms = 5, MolarVolume = 24.37, MolarWeight = 101.96,
                KZero = 249.0, K1Prime = 4.0, GZero = 93.0, G1Prime = 2.0,
                DebyeTempZero = 820.0, GammaZero = 1.48, QZero = 1.1, EhtaZero = 3.0,
                F0 = -1521.040, RefTemp = 300.0,
            },

            // ==================== Ferropericlase system ====================
            new MineralParams
            {
                MineralName = "Periclase", PaperName = "pe",
                NumAtoms = 2, MolarVolume = 11.24, MolarWeight = 40.30,
                KZero = 161.0, K1Prime = 3.8, GZero = 131.0, G1Prime = 2.1,
                DebyeTempZero = 767.0, GammaZero = 1.36, QZero = 1.7, EhtaZero = 2.8,
                F0 = -569.204, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Wuestite", PaperName = "wu",
                NumAtoms = 2, MolarVolume = 12.26, MolarWeight = 71.84,
                KZero = 152.0, K1Prime = 4.9, GZero = 59.0, G1Prime = 1.4,
                DebyeTempZero = 454.0, GammaZero = 1.53, QZero = 1.7, EhtaZero = 0.6,
                F0 = -242.660, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },

            // ==================== SiO2 polymorphs ====================
            new MineralParams
            {
                MineralName = "Quartz", PaperName = "qtz",
                NumAtoms = 3, MolarVolume = 23.71, MolarWeight = 60.08,
                KZero = 49.5, K1Prime = 0.5, GZero = 44.0, G1Prime = 0.5,
                DebyeTempZero = 816.0, GammaZero = 0.0, QZero = 1.0, EhtaZero = 2.4,
                F0 = -856.288, RefTemp = 300.0,
                Tc0 = 847.0, VD = 1.222, SD = 5.164,
            },
            new MineralParams
            {
                MineralName = "Coesite", PaperName = "coe",
                NumAtoms = 3, MolarVolume = 20.66, MolarWeight = 60.08,
                KZero = 114.0, K1Prime = 1.0, GZero = 62.0, G1Prime = 1.0,
                DebyeTempZero = 852.0, GammaZero = 0.39, QZero = 1.0, EhtaZero = 1.4,
                F0 = -852.741, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Stishovite", PaperName = "st",
                NumAtoms = 3, MolarVolume = 14.02, MolarWeight = 60.08,
                KZero = 314.0, K1Prime = 4.4, GZero = 220.0, G1Prime = 1.6,
                DebyeTempZero = 1108.0, GammaZero = 1.37, QZero = 2.8, EhtaZero = 5.0,
                F0 = -815.200, RefTemp = 300.0,
                Tc0 = -4250.0, VD = 0.001, SD = 0.012,
            },
            new MineralParams
            {
                MineralName = "Seifertite", PaperName = "seif",
                NumAtoms = 3, MolarVolume = 13.67, MolarWeight = 60.08,
                KZero = 328.0, K1Prime = 4.0, GZero = 227.0, G1Prime = 1.6,
                DebyeTempZero = 1141.0, GammaZero = 1.37, QZero = 2.8, EhtaZero = 5.0,
                F0 = -803.200, RefTemp = 300.0,
            },

            // ==================== Garnet system ====================
            new MineralParams
            {
                MineralName = "Pyrope", PaperName = "py",
                NumAtoms = 20, MolarVolume = 113.08, MolarWeight = 403.13,
                KZero = 170.0, K1Prime = 4.0, GZero = 94.0, G1Prime = 1.6,
                DebyeTempZero = 823.0, GammaZero = 1.01, QZero = 1.4, EhtaZero = 2.4,
                F0 = -5937.375, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Almandine", PaperName = "al",
                NumAtoms = 20, MolarVolume = 115.28, MolarWeight = 497.75,
                KZero = 175.0, K1Prime = 4.9, GZero = 96.0, G1Prime = 1.4,
                DebyeTempZero = 741.0, GammaZero = 1.06, QZero = 1.4, EhtaZero = 2.1,
                F0 = -4938.062, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 6.0,
            },
            new MineralParams
            {
                MineralName = "Grossular", PaperName = "gr",
                NumAtoms = 20, MolarVolume = 125.12, MolarWeight = 450.45,
                KZero = 168.0, K1Prime = 4.0, GZero = 109.0, G1Prime = 1.2,
                DebyeTempZero = 823.0, GammaZero = 1.05, QZero = 1.9, EhtaZero = 2.4,
                F0 = -6278.135, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Majorite", PaperName = "maj",
                NumAtoms = 20, MolarVolume = 114.32, MolarWeight = 401.58,
                KZero = 165.0, K1Prime = 4.2, GZero = 85.0, G1Prime = 1.4,
                DebyeTempZero = 822.0, GammaZero = 0.98, QZero = 1.5, EhtaZero = 2.2,
                F0 = -5691.790, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Na-Majorite", PaperName = "namaj",
                NumAtoms = 20, MolarVolume = 112.01, MolarWeight = 404.31,
                KZero = 163.0, K1Prime = 5.0, GZero = 103.0, G1Prime = 1.7,
                DebyeTempZero = 825.0, GammaZero = 0.98, QZero = 1.5, EhtaZero = 1.9,
                F0 = -5765.600, RefTemp = 300.0,
            },

            // ==================== Pyroxene system ====================
            // --- Clinopyroxene ---
            new MineralParams
            {
                MineralName = "Diopside", PaperName = "di",
                NumAtoms = 10, MolarVolume = 66.04, MolarWeight = 216.55,
                KZero = 114.0, K1Prime = 4.5, GZero = 67.0, G1Prime = 1.7,
                DebyeTempZero = 782.0, GammaZero = 1.01, QZero = 1.5, EhtaZero = 1.5,
                F0 = -3029.200, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Hedenbergite", PaperName = "he",
                NumAtoms = 10, MolarVolume = 67.87, MolarWeight = 248.09,
                KZero = 119.0, K1Prime = 4.6, GZero = 61.0, G1Prime = 1.4,
                DebyeTempZero = 702.0, GammaZero = 0.96, QZero = 1.5, EhtaZero = 1.1,
                F0 = -2677.700, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },
            new MineralParams
            {
                MineralName = "CaTs", PaperName = "cats",
                NumAtoms = 10, MolarVolume = 63.57, MolarWeight = 218.12,
                KZero = 114.0, K1Prime = 3.9, GZero = 76.0, G1Prime = 1.7,
                DebyeTempZero = 804.0, GammaZero = 0.75, QZero = 3.0, EhtaZero = 1.5,
                F0 = -3120.200, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Jadeite", PaperName = "jd",
                NumAtoms = 10, MolarVolume = 60.40, MolarWeight = 202.14,
                KZero = 142.0, K1Prime = 5.2, GZero = 85.0, G1Prime = 1.4,
                DebyeTempZero = 821.0, GammaZero = 0.90, QZero = 0.4, EhtaZero = 2.6,
                F0 = -2852.100, RefTemp = 300.0,
            },
            // --- Orthopyroxene ---
            new MineralParams
            {
                MineralName = "Enstatite", PaperName = "en",
                NumAtoms = 10, MolarVolume = 62.68, MolarWeight = 200.78,
                KZero = 107.0, K1Prime = 7.0, GZero = 77.0, G1Prime = 1.5,
                DebyeTempZero = 812.0, GammaZero = 0.78, QZero = 3.4, EhtaZero = 1.6,
                F0 = -2913.662, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Ferrosilite", PaperName = "fs",
                NumAtoms = 10, MolarVolume = 65.94, MolarWeight = 263.86,
                KZero = 101.0, K1Prime = 7.0, GZero = 52.0, G1Prime = 1.1,
                DebyeTempZero = 674.0, GammaZero = 0.72, QZero = 3.4, EhtaZero = 0.6,
                F0 = -2227.300, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 2.0,
            },
            new MineralParams
            {
                MineralName = "Mg-Tschermak", PaperName = "mgts",
                NumAtoms = 10, MolarVolume = 59.15, MolarWeight = 202.35,
                KZero = 107.0, K1Prime = 7.0, GZero = 96.0, G1Prime = 1.7,
                DebyeTempZero = 836.0, GammaZero = 0.78, QZero = 3.4, EhtaZero = 2.0,
                F0 = -3003.700, RefTemp = 300.0,
            },

            // ==================== High-pressure pyroxene ====================
            new MineralParams
            {
                MineralName = "HP-Clinoenstatite", PaperName = "hpcen",
                NumAtoms = 10, MolarVolume = 60.50, MolarWeight = 200.78,
                KZero = 117.0, K1Prime = 5.2, GZero = 88.0, G1Prime = 1.6,
                DebyeTempZero = 840.0, GammaZero = 0.85, QZero = 1.5, EhtaZero = 1.6,
                F0 = -2905.350, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "HP-Clinoferrosilite", PaperName = "hpcfs",
                NumAtoms = 10, MolarVolume = 63.64, MolarWeight = 263.86,
                KZero = 115.0, K1Prime = 5.2, GZero = 55.0, G1Prime = 1.1,
                DebyeTempZero = 682.0, GammaZero = 0.85, QZero = 1.5, EhtaZero = 1.6,
                F0 = -2219.600, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 2.0,
            },

            // ==================== Akimotoite ====================
            new MineralParams
            {
                MineralName = "Mg-Akimotoite", PaperName = "mak",
                NumAtoms = 5, MolarVolume = 26.35, MolarWeight = 100.39,
                KZero = 211.0, K1Prime = 5.6, GZero = 132.0, G1Prime = 1.6,
                DebyeTempZero = 886.0, GammaZero = 1.19, QZero = 2.5, EhtaZero = 2.6,
                F0 = -1362.840, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fe-Akimotoite", PaperName = "fak",
                NumAtoms = 5, MolarVolume = 26.85, MolarWeight = 131.93,
                KZero = 211.0, K1Prime = 5.6, GZero = 132.0, G1Prime = 1.6,
                DebyeTempZero = 762.0, GammaZero = 1.19, QZero = 2.5, EhtaZero = 2.6,
                F0 = -1006.100, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },
            new MineralParams
            {
                MineralName = "Corundum", PaperName = "cor",
                NumAtoms = 5, MolarVolume = 25.58, MolarWeight = 101.96,
                KZero = 253.0, K1Prime = 4.3, GZero = 163.0, G1Prime = 1.8,
                DebyeTempZero = 933.0, GammaZero = 1.32, QZero = 1.3, EhtaZero = 2.8,
                F0 = -1582.345, RefTemp = 300.0,
            },

            // ==================== Spinel ====================
            new MineralParams
            {
                MineralName = "Spinel", PaperName = "sp",
                NumAtoms = 7, MolarVolume = 39.78, MolarWeight = 142.27,
                KZero = 198.0, K1Prime = 4.9, GZero = 108.0, G1Prime = 0.5,
                DebyeTempZero = 856.0, GammaZero = 1.02, QZero = 2.6, EhtaZero = 3.1,
                F0 = -2174.440, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Hercynite", PaperName = "hc",
                NumAtoms = 7, MolarVolume = 40.83, MolarWeight = 173.81,
                KZero = 209.0, K1Prime = 5.1, GZero = 84.0, G1Prime = 0.6,
                DebyeTempZero = 762.0, GammaZero = 1.22, QZero = 2.6, EhtaZero = 1.7,
                F0 = -1847.600, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },

            // ==================== Plagioclase (low P) ====================
            new MineralParams
            {
                MineralName = "Anorthite", PaperName = "an",
                NumAtoms = 13, MolarVolume = 100.61, MolarWeight = 278.21,
                KZero = 84.0, K1Prime = 4.1, GZero = 40.0, G1Prime = 1.1,
                DebyeTempZero = 752.0, GammaZero = 0.39, QZero = 1.0, EhtaZero = 3.1,
                F0 = -4007.900, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Albite", PaperName = "ab",
                NumAtoms = 13, MolarVolume = 100.07, MolarWeight = 262.22,
                KZero = 60.0, K1Prime = 4.0, GZero = 36.0, G1Prime = 1.4,
                DebyeTempZero = 752.0, GammaZero = 0.39, QZero = 1.0, EhtaZero = 3.1,
                F0 = -3719.600, RefTemp = 300.0,
            },

            // ==================== Ca-Perovskite ====================
            new MineralParams
            {
                MineralName = "Ca-Perovskite", PaperName = "capv",
                NumAtoms = 5, MolarVolume = 27.45, MolarWeight = 116.16,
                KZero = 236.0, K1Prime = 3.9, GZero = 165.0, G1Prime = 2.5,
                DebyeTempZero = 804.0, GammaZero = 1.89, QZero = 0.9, EhtaZero = 1.3,
                F0 = -1463.130, RefTemp = 300.0,
            },

            // ==================== Feldspar high-P ====================
            new MineralParams
            {
                MineralName = "Nepheline", PaperName = "neph",
                NumAtoms = 7, MolarVolume = 54.16, MolarWeight = 142.05,
                KZero = 53.0, K1Prime = 5.0, GZero = 31.0, G1Prime = 1.1,
                DebyeTempZero = 700.0, GammaZero = 0.39, QZero = 1.0, EhtaZero = 3.1,
                F0 = -1967.300, RefTemp = 300.0,
            },

            // ==================== CaFerrite-type ====================
            new MineralParams
            {
                MineralName = "Mg-CaFerrite", PaperName = "mcf",
                NumAtoms = 7, MolarVolume = 36.24, MolarWeight = 142.27,
                KZero = 212.0, K1Prime = 4.1, GZero = 129.0, G1Prime = 1.7,
                DebyeTempZero = 846.0, GammaZero = 1.30, QZero = 1.3, EhtaZero = 3.0,
                F0 = -2119.450, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fe-CaFerrite", PaperName = "fcf",
                NumAtoms = 7, MolarVolume = 36.70, MolarWeight = 173.81,
                KZero = 212.0, K1Prime = 4.1, GZero = 129.0, G1Prime = 1.7,
                DebyeTempZero = 740.0, GammaZero = 1.30, QZero = 1.3, EhtaZero = 3.0,
                F0 = -1793.600, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },
            new MineralParams
            {
                MineralName = "Na-CaFerrite", PaperName = "ncf",
                NumAtoms = 7, MolarVolume = 36.58, MolarWeight = 142.05,
                KZero = 218.0, K1Prime = 4.0, GZero = 128.0, G1Prime = 1.7,
                DebyeTempZero = 836.0, GammaZero = 1.30, QZero = 1.3, EhtaZero = 3.0,
                F0 = -1903.900, RefTemp = 300.0,
            },

            // ==================== Other phases ====================
            new MineralParams
            {
                MineralName = "Kyanite", PaperName = "ky",
                NumAtoms = 8, MolarVolume = 44.22, MolarWeight = 162.05,
                KZero = 160.0, K1Prime = 4.0, GZero = 121.0, G1Prime = 0.9,
                DebyeTempZero = 943.0, GammaZero = 0.93, QZero = 0.4, EhtaZero = 2.0,
                F0 = -2444.600, RefTemp = 300.0,
            },

            new MineralParams
            {
                MineralName = "Mg-Ilmenite", PaperName = "mil",
                NumAtoms = 5, MolarVolume = 26.35, MolarWeight = 100.39,
                KZero = 211.0, K1Prime = 5.6, GZero = 132.0, G1Prime = 1.6,
                DebyeTempZero = 886.0, GammaZero = 1.19, QZero = 2.5, EhtaZero = 2.6,
                F0 = -1362.840, RefTemp = 300.0,
            },
        };
    }
}
