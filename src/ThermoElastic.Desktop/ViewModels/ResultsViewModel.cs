using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class ResultsViewModel : ObservableObject
{
    public ObservableCollection<ResultSummary> Results { get; } = new();

    public void SetResults(IEnumerable<ResultSummary> results)
    {
        Results.Clear();
        foreach (var r in results)
        {
            Results.Add(r);
        }
    }
}
