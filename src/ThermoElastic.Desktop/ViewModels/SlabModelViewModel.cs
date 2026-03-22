using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public partial class SlabModelViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private double _plateAgeMyr = 80.0;
    [ObservableProperty] private double _pressure = 13.0;
    [ObservableProperty] private double _slabT = 1000.0;
    [ObservableProperty] private double _ambientT = 1700.0;
    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private double _dVsPercent;
    [ObservableProperty] private double _dRhoPercent;

    public ObservableCollection<GeothermPoint> GeothermResults { get; } = new();

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    public class GeothermPoint
    {
        public double Depth { get; set; }
        public double Temperature { get; set; }
        public double Pressure { get; set; }
    }

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mineral = minerals[SelectedMineralIndex];

            var model = new SlabThermalModel();
            var geotherm = model.ComputeSlabGeotherm(PlateAgeMyr);

            GeothermResults.Clear();
            foreach (var pt in geotherm)
            {
                GeothermResults.Add(new GeothermPoint
                {
                    Depth = pt.Depth_km,
                    Temperature = pt.Temperature_K,
                    Pressure = pt.Pressure_GPa
                });
            }

            var anomaly = model.ComputeSlabAnomaly(mineral, Pressure, SlabT, AmbientT);
            DVsPercent = anomaly.dVs_percent;
            DRhoPercent = anomaly.dRho_percent;

            StatusMessage = $"Slab geotherm computed ({geotherm.Count} points), dVs = {DVsPercent:F2}%";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
