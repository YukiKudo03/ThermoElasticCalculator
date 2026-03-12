using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class ChartViewModel : ObservableObject
{
    [ObservableProperty]
    private string _chartTitle = "Chart View";

    [ObservableProperty]
    private string _xAxisLabel = "Pressure [GPa]";

    [ObservableProperty]
    private string _yAxisLabel = "Velocity [m/s]";

    [ObservableProperty]
    private string _statusMessage = "Load results to display chart.";

    public ObservableCollection<ChartDataPoint> DataPoints { get; } = new();

    public void SetResultsData(List<ResultSummary> results, string propertyName)
    {
        DataPoints.Clear();
        foreach (var r in results)
        {
            double yValue = propertyName switch
            {
                "Vp" => r.Vp,
                "Vs" => r.Vs,
                "Vb" => r.Vb,
                "Density" => r.Density,
                "KS" => r.KS,
                "GS" => r.GS,
                _ => r.Vp,
            };
            DataPoints.Add(new ChartDataPoint(r.GivenP, r.GivenT, yValue, propertyName));
        }
        ChartTitle = $"{propertyName} vs Pressure";
        StatusMessage = $"{DataPoints.Count} data points loaded.";
    }
}

public record ChartDataPoint(double Pressure, double Temperature, double Value, string PropertyName);
