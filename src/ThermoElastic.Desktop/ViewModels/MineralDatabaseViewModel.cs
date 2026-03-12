using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class MineralDatabaseViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private MineralParams? _selectedMineral;

    public ObservableCollection<MineralParams> Minerals { get; } = new();

    public MineralDatabaseViewModel()
    {
        LoadAllMinerals();
    }

    private void LoadAllMinerals()
    {
        Minerals.Clear();
        foreach (var m in MineralDatabase.GetAll())
            Minerals.Add(m);
    }

    [RelayCommand]
    private void Search()
    {
        Minerals.Clear();
        var results = string.IsNullOrWhiteSpace(SearchText)
            ? MineralDatabase.GetAll()
            : MineralDatabase.Search(SearchText);
        foreach (var m in results)
            Minerals.Add(m);
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        LoadAllMinerals();
    }

    public MineralParams? GetSelectedMineralCopy()
    {
        if (SelectedMineral == null) return null;
        var json = SelectedMineral.ExportJson();
        MineralParams.ImportJson(json, out var copy);
        return copy;
    }
}
