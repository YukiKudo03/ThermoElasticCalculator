"""
Generate reference data from BurnMan (SLB2011) for verification against ThermoElasticCalculator.
Outputs CSV files with computed mineral properties at various P-T conditions.
BurnMan v2.1 API.
"""
import csv
import numpy as np
import burnman
from burnman import minerals

# ============================================================
# 1. Endmember minerals: map SLB2011 paper names to BurnMan classes
# ============================================================
endmember_map = {
    "fo":   minerals.SLB_2011.forsterite,
    "fa":   minerals.SLB_2011.fayalite,
    "mw":   minerals.SLB_2011.mg_wadsleyite,
    "fw":   minerals.SLB_2011.fe_wadsleyite,
    "mrw":  minerals.SLB_2011.mg_ringwoodite,
    "frw":  minerals.SLB_2011.fe_ringwoodite,
    "mpv":  minerals.SLB_2011.mg_perovskite,
    "fpv":  minerals.SLB_2011.fe_perovskite,
    "apv":  minerals.SLB_2011.al_perovskite,
    "mppv": minerals.SLB_2011.mg_post_perovskite,
    "fppv": minerals.SLB_2011.fe_post_perovskite,
    "appv": minerals.SLB_2011.al_post_perovskite,
    "pe":   minerals.SLB_2011.periclase,
    "wu":   minerals.SLB_2011.wuestite,
    "qtz":  minerals.SLB_2011.quartz,
    "coe":  minerals.SLB_2011.coesite,
    "st":   minerals.SLB_2011.stishovite,
    "seif": minerals.SLB_2011.seifertite,
    "py":   minerals.SLB_2011.pyrope,
    "al":   minerals.SLB_2011.almandine,
    "gr":   minerals.SLB_2011.grossular,
    "maj":  minerals.SLB_2011.mg_majorite,
    "di":   minerals.SLB_2011.diopside,
    "he":   minerals.SLB_2011.hedenbergite,
    "cats": minerals.SLB_2011.ca_tschermaks,
    "jd":   minerals.SLB_2011.jadeite,
    "en":   minerals.SLB_2011.enstatite,
    "fs":   minerals.SLB_2011.ferrosilite,
    "mgts": minerals.SLB_2011.mg_tschermaks,
    "hpcen": minerals.SLB_2011.hp_clinoenstatite,
    "hpcfs": minerals.SLB_2011.hp_clinoferrosilite,
    "mak":  minerals.SLB_2011.mg_akimotoite,
    "fak":  minerals.SLB_2011.fe_akimotoite,
    "cor":  minerals.SLB_2011.corundum,
    "sp":   minerals.SLB_2011.spinel,
    "hc":   minerals.SLB_2011.hercynite,
    "an":   minerals.SLB_2011.anorthite,
    "ab":   minerals.SLB_2011.albite,
    "capv": minerals.SLB_2011.ca_perovskite,
    "neph": minerals.SLB_2011.nepheline,
    "ky":   minerals.SLB_2011.kyanite,
    "mil":  minerals.SLB_2011.mgil,
}

# P-T grid for endmember verification
pressures_GPa = [0.0001, 5.0, 10.0, 25.0, 50.0, 100.0, 135.0]
temperatures_K = [300.0, 1000.0, 1500.0, 2000.0, 2500.0]

print("Generating endmember reference data...")

with open("burnman_endmember_reference.csv", "w", newline="") as f:
    writer = csv.writer(f)
    writer.writerow([
        "PaperName", "P_GPa", "T_K",
        "rho_kg_m3", "V_m3_mol",
        "KS_GPa", "KT_GPa", "G_GPa",
        "Vp_m_s", "Vs_m_s", "Vb_m_s",
        "alpha_1_K", "Cv_J_mol_K", "Cp_J_mol_K",
        "gamma", "F_J_mol", "gibbs_J_mol", "entropy_J_mol_K"
    ])

    for name, mineral_cls in endmember_map.items():
        for P_GPa in pressures_GPa:
            for T_K in temperatures_K:
                P_Pa = P_GPa * 1e9
                try:
                    m = mineral_cls()
                    m.set_state(P_Pa, T_K)

                    writer.writerow([
                        name, P_GPa, T_K,
                        f"{m.rho:.6f}", f"{m.V:.10e}",
                        f"{m.K_S/1e9:.6f}", f"{m.K_T/1e9:.6f}", f"{m.G/1e9:.6f}",
                        f"{m.v_p:.4f}", f"{m.shear_wave_velocity:.4f}", f"{m.bulk_sound_velocity:.4f}",
                        f"{m.alpha:.8e}", f"{m.C_v:.6f}", f"{m.C_p:.6f}",
                        f"{m.gr:.6f}", f"{m.molar_helmholtz:.4f}", f"{m.molar_gibbs:.4f}", f"{m.S:.6f}"
                    ])
                except Exception as e:
                    print(f"  WARN: {name} at P={P_GPa} GPa, T={T_K} K failed: {e}")

print("Endmember reference data saved to burnman_endmember_reference.csv")

# ============================================================
# 2. Solid solution reference data
# ============================================================
print("\nGenerating solid solution reference data...")

with open("burnman_solution_reference.csv", "w", newline="") as f:
    writer = csv.writer(f)
    writer.writerow([
        "SolutionName", "Composition", "P_GPa", "T_K",
        "rho_kg_m3", "KS_GPa", "G_GPa",
        "Vp_m_s", "Vs_m_s",
        "gibbs_J_mol", "entropy_J_mol_K",
        "excess_gibbs_J_mol"
    ])

    # Olivine (fo-fa) system
    olivine_compositions = [
        ("Fo100", [1.0, 0.0]),
        ("Fo90Fa10", [0.9, 0.1]),
        ("Fo80Fa20", [0.8, 0.2]),
        ("Fo50Fa50", [0.5, 0.5]),
    ]
    pt_conditions = [
        (0.0001, 300.0), (5.0, 1500.0), (10.0, 1500.0), (10.0, 2000.0),
    ]

    for comp_name, molar_fractions in olivine_compositions:
        for P_GPa, T_K in pt_conditions:
            try:
                ol = minerals.SLB_2011.mg_fe_olivine()
                ol.set_composition(molar_fractions)
                ol.set_state(P_GPa * 1e9, T_K)

                excess_gibbs = ol.excess_gibbs

                writer.writerow([
                    "olivine", comp_name, P_GPa, T_K,
                    f"{ol.rho:.6f}",
                    f"{ol.K_S/1e9:.6f}",
                    f"{ol.G/1e9:.6f}",
                    f"{ol.v_p:.4f}",
                    f"{ol.shear_wave_velocity:.4f}",
                    f"{ol.molar_gibbs:.4f}",
                    f"{ol.S:.6f}",
                    f"{excess_gibbs:.4f}"
                ])
            except Exception as e:
                print(f"  WARN: olivine {comp_name} at {P_GPa},{T_K}: {e}")

    # Ferropericlase (pe-wu) system
    fp_compositions = [
        ("Pe100", [1.0, 0.0]),
        ("Pe80Wu20", [0.8, 0.2]),
        ("Pe50Wu50", [0.5, 0.5]),
    ]
    fp_conditions = [
        (0.0001, 300.0), (25.0, 1800.0), (50.0, 2000.0), (100.0, 2500.0),
    ]

    for comp_name, molar_fractions in fp_compositions:
        for P_GPa, T_K in fp_conditions:
            try:
                fp = minerals.SLB_2011.ferropericlase()
                fp.set_composition(molar_fractions)
                fp.set_state(P_GPa * 1e9, T_K)

                writer.writerow([
                    "ferropericlase", comp_name, P_GPa, T_K,
                    f"{fp.rho:.6f}",
                    f"{fp.K_S/1e9:.6f}",
                    f"{fp.G/1e9:.6f}",
                    f"{fp.v_p:.4f}",
                    f"{fp.shear_wave_velocity:.4f}",
                    f"{fp.molar_gibbs:.4f}",
                    f"{fp.S:.6f}",
                    f"{fp.excess_gibbs:.4f}"
                ])
            except Exception as e:
                print(f"  WARN: fp {comp_name} at {P_GPa},{T_K}: {e}")

print("Solid solution reference data saved to burnman_solution_reference.csv")

# ============================================================
# 3. Debye function reference values
# ============================================================
print("\nGenerating Debye function reference data...")

from burnman.utils.math import debye_fn

with open("burnman_debye_reference.csv", "w", newline="") as f:
    writer = csv.writer(f)
    writer.writerow(["x", "D3_x"])

    x_values = [0.001, 0.01, 0.1, 0.5, 1.0, 2.0, 3.0, 5.0, 10.0, 20.0, 50.0, 100.0]
    for x in x_values:
        D3 = debye_fn(x)
        writer.writerow([f"{x}", f"{D3:.15e}"])

print("Debye function reference data saved to burnman_debye_reference.csv")

print("\n=== All reference data generation complete ===")
