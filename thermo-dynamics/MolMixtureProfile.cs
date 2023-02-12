using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thermo_dynamics
{
    public class MolProfile
    {
        public MolProfile(List<(double MineralMolRatio, MineralParams Mineral)> elem1MolRatio, PTProfile ptProfile)
        {
            MineralMolRatios = elem1MolRatio;
            PTProfile = ptProfile;
        }

        public List<(double MineralMolRatio, MineralParams Mineral)> MineralMolRatios { get; set; }
        public PTProfile PTProfile { get; set; }
        public List<MixtureCalculator> ConvertToVProfiles()
        {
            return PTProfile.Profile.Select(pt =>
            {
                var tmp = MineralMolRatios.Select(ratio => (MolRatio: ratio.MineralMolRatio, MineralElemSummary: new MieGruneisenEOSOptimizer(ratio.Mineral, pt).ExecOptimize().ExportResults()));
                var volSum = tmp.Sum(t => t.MolRatio * t.MineralElemSummary.Volume);
                return new MixtureCalculator(tmp.Select(t => (t.MineralElemSummary.Volume * t.MolRatio / volSum, t.MineralElemSummary)).ToList());
            }).ToList();
        }
    }
}
