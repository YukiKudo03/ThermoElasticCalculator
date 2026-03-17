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
                KZero = 127.96, K1Prime = 4.2180, GZero = 81.60, G1Prime = 1.4626,
                DebyeTempZero = 809.17, GammaZero = 0.99282, QZero = 2.10672, EhtaZero = 2.2997,
                F0 = -2055.403, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fayalite", PaperName = "fa",
                NumAtoms = 7, MolarVolume = 46.290, MolarWeight = 203.77,
                KZero = 134.96, K1Prime = 4.2180, GZero = 50.90, G1Prime = 1.4626,
                DebyeTempZero = 618.70, GammaZero = 1.06023, QZero = 3.64660, EhtaZero = 1.0250,
                F0 = -1370.519, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 2.0,
            },

            // ==================== Wadsleyite system ====================
            new MineralParams
            {
                MineralName = "Mg-Wadsleyite", PaperName = "mw",
                NumAtoms = 7, MolarVolume = 40.515, MolarWeight = 140.69,
                KZero = 168.69, K1Prime = 4.3229, GZero = 112.00, G1Prime = 1.4442,
                DebyeTempZero = 843.50, GammaZero = 1.20610, QZero = 2.01880, EhtaZero = 2.6368,
                F0 = -2027.837, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fe-Wadsleyite", PaperName = "fw",
                NumAtoms = 7, MolarVolume = 42.800, MolarWeight = 203.77,
                KZero = 168.59, K1Prime = 4.3229, GZero = 72.00, G1Prime = 1.4442,
                DebyeTempZero = 665.45, GammaZero = 1.20610, QZero = 2.01880, EhtaZero = 1.0402,
                F0 = -1364.668, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 8.0,
            },

            // ==================== Ringwoodite system ====================
            new MineralParams
            {
                MineralName = "Mg-Ringwoodite", PaperName = "mrw",
                NumAtoms = 7, MolarVolume = 39.493, MolarWeight = 140.69,
                KZero = 184.90, K1Prime = 4.2203, GZero = 123.00, G1Prime = 1.3541,
                DebyeTempZero = 877.71, GammaZero = 1.10791, QZero = 2.39140, EhtaZero = 2.3046,
                F0 = -2017.557, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fe-Ringwoodite", PaperName = "frw",
                NumAtoms = 7, MolarVolume = 41.860, MolarWeight = 203.77,
                KZero = 213.41, K1Prime = 4.2203, GZero = 92.00, G1Prime = 1.3541,
                DebyeTempZero = 677.72, GammaZero = 1.27193, QZero = 2.39140, EhtaZero = 1.7725,
                F0 = -1362.772, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 2.0,
            },

            // ==================== Perovskite (Bridgmanite) system ====================
            new MineralParams
            {
                MineralName = "Mg-Perovskite", PaperName = "mpv",
                NumAtoms = 5, MolarVolume = 24.445, MolarWeight = 100.39,
                KZero = 250.53, K1Prime = 4.1400, GZero = 172.90, G1Prime = 1.6904,
                DebyeTempZero = 905.94, GammaZero = 1.56508, QZero = 1.10945, EhtaZero = 2.5654,
                F0 = -1368.283, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fe-Perovskite", PaperName = "fpv",
                NumAtoms = 5, MolarVolume = 25.485, MolarWeight = 131.93,
                KZero = 272.12, K1Prime = 4.1400, GZero = 132.68, G1Prime = 1.3748,
                DebyeTempZero = 870.81, GammaZero = 1.56508, QZero = 1.10945, EhtaZero = 2.2921,
                F0 = -1040.920, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },
            new MineralParams
            {
                MineralName = "Al-Perovskite", PaperName = "apv",
                NumAtoms = 5, MolarVolume = 24.944, MolarWeight = 101.96,
                KZero = 258.20, K1Prime = 4.1400, GZero = 171.31, G1Prime = 1.4971,
                DebyeTempZero = 886.46, GammaZero = 1.56508, QZero = 1.10945, EhtaZero = 2.4713,
                F0 = -1533.878, RefTemp = 300.0,
            },

            // ==================== Post-Perovskite ====================
            new MineralParams
            {
                MineralName = "Mg-PostPerovskite", PaperName = "mppv",
                NumAtoms = 5, MolarVolume = 24.419, MolarWeight = 100.39,
                KZero = 231.20, K1Prime = 4.0000, GZero = 150.17, G1Prime = 1.9787,
                DebyeTempZero = 855.82, GammaZero = 1.89155, QZero = 1.09081, EhtaZero = 1.1670,
                F0 = -1348.641, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fe-PostPerovskite", PaperName = "fppv",
                NumAtoms = 5, MolarVolume = 25.459, MolarWeight = 131.93,
                KZero = 231.20, K1Prime = 4.0000, GZero = 129.50, G1Prime = 1.4467,
                DebyeTempZero = 781.35, GammaZero = 1.89155, QZero = 1.09081, EhtaZero = 1.3638,
                F0 = -981.807, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },
            new MineralParams
            {
                MineralName = "Al-PostPerovskite", PaperName = "appv",
                NumAtoms = 5, MolarVolume = 23.847, MolarWeight = 101.96,
                KZero = 249.00, K1Prime = 4.0000, GZero = 91.97, G1Prime = 1.8160,
                DebyeTempZero = 762.20, GammaZero = 1.64573, QZero = 1.09081, EhtaZero = 2.8376,
                F0 = -1377.582, RefTemp = 300.0,
            },

            // ==================== Ferropericlase system ====================
            new MineralParams
            {
                MineralName = "Periclase", PaperName = "pe",
                NumAtoms = 2, MolarVolume = 11.244, MolarWeight = 40.30,
                KZero = 161.38, K1Prime = 3.8405, GZero = 130.90, G1Prime = 2.1438,
                DebyeTempZero = 767.10, GammaZero = 1.36127, QZero = 1.72170, EhtaZero = 2.8176,
                F0 = -569.445, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Wuestite", PaperName = "wu",
                NumAtoms = 2, MolarVolume = 12.264, MolarWeight = 71.84,
                KZero = 179.44, K1Prime = 4.9376, GZero = 59.00, G1Prime = 1.4467,
                DebyeTempZero = 454.16, GammaZero = 1.53047, QZero = 1.72170, EhtaZero = -0.0573,
                F0 = -242.146, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },

            // ==================== SiO2 polymorphs ====================
            new MineralParams
            {
                MineralName = "Quartz", PaperName = "qtz",
                NumAtoms = 3, MolarVolume = 23.670, MolarWeight = 60.08,
                KZero = 49.55, K1Prime = 4.3316, GZero = 44.86, G1Prime = 0.9532,
                DebyeTempZero = 816.33, GammaZero = -0.00296, QZero = 1.00000, EhtaZero = 2.3647,
                F0 = -858.853, RefTemp = 300.0,
                Tc0 = 847.0, VD = 1.222, SD = 5.164,
            },
            new MineralParams
            {
                MineralName = "Coesite", PaperName = "coe",
                NumAtoms = 3, MolarVolume = 20.657, MolarWeight = 60.08,
                KZero = 113.59, K1Prime = 4.0000, GZero = 61.60, G1Prime = 1.2473,
                DebyeTempZero = 852.43, GammaZero = 0.39157, QZero = 1.00000, EhtaZero = 2.3979,
                F0 = -855.068, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Stishovite", PaperName = "st",
                NumAtoms = 3, MolarVolume = 14.017, MolarWeight = 60.08,
                KZero = 314.34, K1Prime = 3.7512, GZero = 220.00, G1Prime = 1.9333,
                DebyeTempZero = 1107.82, GammaZero = 1.37466, QZero = 2.83517, EhtaZero = 4.6090,
                F0 = -818.985, RefTemp = 300.0,
                Tc0 = -4250.0, VD = 0.001, SD = 0.012,
            },
            new MineralParams
            {
                MineralName = "Seifertite", PaperName = "seif",
                NumAtoms = 3, MolarVolume = 13.670, MolarWeight = 60.08,
                KZero = 327.58, K1Prime = 4.0155, GZero = 227.45, G1Prime = 1.7696,
                DebyeTempZero = 1140.77, GammaZero = 1.37466, QZero = 2.83517, EhtaZero = 4.9711,
                F0 = -794.335, RefTemp = 300.0,
            },

            // ==================== Garnet system ====================
            new MineralParams
            {
                MineralName = "Pyrope", PaperName = "py",
                NumAtoms = 20, MolarVolume = 113.080, MolarWeight = 403.13,
                KZero = 170.24, K1Prime = 4.1107, GZero = 93.70, G1Prime = 1.3576,
                DebyeTempZero = 823.21, GammaZero = 1.01424, QZero = 1.42169, EhtaZero = 0.9819,
                F0 = -5936.538, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Almandine", PaperName = "al",
                NumAtoms = 20, MolarVolume = 115.430, MolarWeight = 497.75,
                KZero = 173.90, K1Prime = 4.9134, GZero = 96.00, G1Prime = 1.4093,
                DebyeTempZero = 741.36, GammaZero = 1.06495, QZero = 1.42169, EhtaZero = 2.0929,
                F0 = -4935.516, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 6.0,
            },
            new MineralParams
            {
                MineralName = "Grossular", PaperName = "gr",
                NumAtoms = 20, MolarVolume = 125.120, MolarWeight = 450.45,
                KZero = 167.06, K1Prime = 3.9154, GZero = 109.00, G1Prime = 1.1627,
                DebyeTempZero = 822.74, GammaZero = 1.05404, QZero = 1.88887, EhtaZero = 2.3842,
                F0 = -6277.935, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Majorite", PaperName = "maj",
                NumAtoms = 20, MolarVolume = 114.324, MolarWeight = 401.55,
                KZero = 165.12, K1Prime = 4.2118, GZero = 85.00, G1Prime = 1.4297,
                DebyeTempZero = 822.46, GammaZero = 0.97682, QZero = 1.53581, EhtaZero = 1.0178,
                F0 = -5691.614, RefTemp = 300.0,
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
                NumAtoms = 10, MolarVolume = 66.039, MolarWeight = 216.55,
                KZero = 112.24, K1Prime = 5.2389, GZero = 67.00, G1Prime = 1.3729,
                DebyeTempZero = 781.61, GammaZero = 0.95873, QZero = 1.52852, EhtaZero = 1.5735,
                F0 = -3029.531, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Hedenbergite", PaperName = "he",
                NumAtoms = 10, MolarVolume = 67.867, MolarWeight = 248.09,
                KZero = 119.26, K1Prime = 5.2389, GZero = 61.00, G1Prime = 1.1765,
                DebyeTempZero = 701.59, GammaZero = 0.93516, QZero = 1.52852, EhtaZero = 1.5703,
                F0 = -2677.330, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },
            new MineralParams
            {
                MineralName = "CaTs", PaperName = "cats",
                NumAtoms = 10, MolarVolume = 63.574, MolarWeight = 218.12,
                KZero = 112.24, K1Prime = 5.2389, GZero = 75.16, G1Prime = 1.5402,
                DebyeTempZero = 803.66, GammaZero = 0.78126, QZero = 1.52852, EhtaZero = 1.9672,
                F0 = -3120.253, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Jadeite", PaperName = "jd",
                NumAtoms = 10, MolarVolume = 60.508, MolarWeight = 202.14,
                KZero = 142.29, K1Prime = 5.2389, GZero = 85.00, G1Prime = 1.3740,
                DebyeTempZero = 820.76, GammaZero = 0.90300, QZero = 0.39234, EhtaZero = 2.1845,
                F0 = -2855.192, RefTemp = 300.0,
            },
            // --- Orthopyroxene ---
            new MineralParams
            {
                MineralName = "Enstatite", PaperName = "en",
                NumAtoms = 10, MolarVolume = 62.676, MolarWeight = 200.78,
                KZero = 107.08, K1Prime = 7.0275, GZero = 76.80, G1Prime = 1.5460,
                DebyeTempZero = 812.18, GammaZero = 0.78479, QZero = 3.43846, EhtaZero = 2.5045,
                F0 = -2913.596, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Ferrosilite", PaperName = "fs",
                NumAtoms = 10, MolarVolume = 65.941, MolarWeight = 263.86,
                KZero = 100.54, K1Prime = 7.0275, GZero = 52.00, G1Prime = 1.5460,
                DebyeTempZero = 674.48, GammaZero = 0.71889, QZero = 3.43846, EhtaZero = 1.0771,
                F0 = -2225.718, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 2.0,
            },
            new MineralParams
            {
                MineralName = "Mg-Tschermak", PaperName = "mgts",
                NumAtoms = 10, MolarVolume = 59.140, MolarWeight = 202.35,
                KZero = 107.08, K1Prime = 7.0275, GZero = 95.95, G1Prime = 1.5460,
                DebyeTempZero = 783.84, GammaZero = 0.78479, QZero = 3.43846, EhtaZero = 2.4910,
                F0 = -3002.470, RefTemp = 300.0,
            },

            // ==================== High-pressure pyroxene ====================
            new MineralParams
            {
                MineralName = "HP-Clinoenstatite", PaperName = "hpcen",
                NumAtoms = 10, MolarVolume = 60.760, MolarWeight = 200.78,
                KZero = 116.03, K1Prime = 6.2368, GZero = 87.93, G1Prime = 1.8412,
                DebyeTempZero = 824.44, GammaZero = 1.12473, QZero = 0.20401, EhtaZero = 2.1418,
                F0 = -2905.788, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "HP-Clinoferrosilite", PaperName = "hpcfs",
                NumAtoms = 10, MolarVolume = 63.854, MolarWeight = 263.86,
                KZero = 116.03, K1Prime = 6.2368, GZero = 70.62, G1Prime = 1.8412,
                DebyeTempZero = 691.56, GammaZero = 1.12473, QZero = 0.20401, EhtaZero = 0.7922,
                F0 = -2222.183, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 2.0,
            },

            // ==================== Akimotoite ====================
            new MineralParams
            {
                MineralName = "Mg-Akimotoite", PaperName = "mak",
                NumAtoms = 5, MolarVolume = 26.354, MolarWeight = 100.39,
                KZero = 210.71, K1Prime = 5.6209, GZero = 132.00, G1Prime = 1.5789,
                DebyeTempZero = 935.98, GammaZero = 1.18984, QZero = 2.34514, EhtaZero = 2.8078,
                F0 = -1410.850, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Fe-Akimotoite", PaperName = "fak",
                NumAtoms = 5, MolarVolume = 26.854, MolarWeight = 131.93,
                KZero = 210.71, K1Prime = 5.6209, GZero = 152.30, G1Prime = 1.5789,
                DebyeTempZero = 887.87, GammaZero = 1.18984, QZero = 2.34514, EhtaZero = 3.5716,
                F0 = -1067.598, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },
            new MineralParams
            {
                MineralName = "Corundum", PaperName = "cor",
                NumAtoms = 5, MolarVolume = 25.577, MolarWeight = 101.96,
                KZero = 252.55, K1Prime = 4.3373, GZero = 163.20, G1Prime = 1.6417,
                DebyeTempZero = 932.57, GammaZero = 1.32442, QZero = 1.30316, EhtaZero = 2.8316,
                F0 = -1582.454, RefTemp = 300.0,
            },

            // ==================== Spinel ====================
            new MineralParams
            {
                MineralName = "Spinel", PaperName = "sp",
                NumAtoms = 28, MolarVolume = 159.048, MolarWeight = 569.06,
                KZero = 196.94, K1Prime = 5.6828, GZero = 108.50, G1Prime = 0.3730,
                DebyeTempZero = 842.81, GammaZero = 1.02283, QZero = 2.71208, EhtaZero = 2.6628,
                F0 = -8667.568, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Hercynite", PaperName = "hc",
                NumAtoms = 28, MolarVolume = 163.372, MolarWeight = 695.22,
                KZero = 208.90, K1Prime = 5.6828, GZero = 84.50, G1Prime = 0.3730,
                DebyeTempZero = 763.23, GammaZero = 1.21719, QZero = 2.71208, EhtaZero = 2.7680,
                F0 = -7324.009, RefTemp = 300.0,
                SpinQuantumNumber = 2.0, MagneticAtomCount = 1.0,
            },

            // ==================== Plagioclase (low P) ====================
            new MineralParams
            {
                MineralName = "Anorthite", PaperName = "an",
                NumAtoms = 13, MolarVolume = 100.610, MolarWeight = 278.21,
                KZero = 84.09, K1Prime = 4.0000, GZero = 39.90, G1Prime = 1.0913,
                DebyeTempZero = 752.39, GammaZero = 0.39241, QZero = 1.00000, EhtaZero = 1.6254,
                F0 = -4014.619, RefTemp = 300.0,
            },
            new MineralParams
            {
                MineralName = "Albite", PaperName = "ab",
                NumAtoms = 13, MolarVolume = 100.452, MolarWeight = 262.22,
                KZero = 59.76, K1Prime = 4.0000, GZero = 36.00, G1Prime = 1.3855,
                DebyeTempZero = 713.78, GammaZero = 0.56704, QZero = 1.00000, EhtaZero = 1.0421,
                F0 = -3718.799, RefTemp = 300.0,
            },

            // ==================== Ca-Perovskite ====================
            new MineralParams
            {
                MineralName = "Ca-Perovskite", PaperName = "capv",
                NumAtoms = 5, MolarVolume = 27.450, MolarWeight = 116.16,
                KZero = 236.00, K1Prime = 3.9000, GZero = 156.83, G1Prime = 2.2271,
                DebyeTempZero = 795.78, GammaZero = 1.88839, QZero = 0.89769, EhtaZero = 1.2882,
                F0 = -1463.358, RefTemp = 300.0,
            },

            // ==================== Feldspar high-P ====================
            new MineralParams
            {
                MineralName = "Nepheline", PaperName = "neph",
                NumAtoms = 7, MolarVolume = 54.668, MolarWeight = 142.05,
                KZero = 53.08, K1Prime = 4.0000, GZero = 30.70, G1Prime = 1.3303,
                DebyeTempZero = 700.94, GammaZero = 0.69428, QZero = 1.00000, EhtaZero = 0.6291,
                F0 = -1992.104, RefTemp = 300.0,
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
                NumAtoms = 8, MolarVolume = 44.227, MolarWeight = 162.05,
                KZero = 160.00, K1Prime = 4.0000, GZero = 120.40, G1Prime = 1.7308,
                DebyeTempZero = 943.17, GammaZero = 0.92550, QZero = 1.00000, EhtaZero = 2.9667,
                F0 = -2446.058, RefTemp = 300.0,
            },

            new MineralParams
            {
                MineralName = "Mg-Ilmenite", PaperName = "mil",
                NumAtoms = 5, MolarVolume = 26.354, MolarWeight = 100.39,
                KZero = 210.71, K1Prime = 5.6209, GZero = 132.00, G1Prime = 1.5789,
                DebyeTempZero = 935.98, GammaZero = 1.18984, QZero = 2.34514, EhtaZero = 2.8078,
                F0 = -1410.850, RefTemp = 300.0,
            },
        };
    }
}
