using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.IO;
using ThermoElastic.Core.Models;
using ThermoElastic.Desktop.ViewModels;

namespace ThermoElastic.Desktop.Views;

public partial class MixtureView : UserControl
{
    public MixtureView()
    {
        InitializeComponent();
    }

    private MixtureViewModel? ViewModel => DataContext as MixtureViewModel;

    private async void OnLoadMineral1(object? sender, RoutedEventArgs e)
    {
        var mineral = await LoadMineralFile();
        if (mineral != null) ViewModel?.LoadMineral1(mineral);
    }

    private async void OnLoadMineral2(object? sender, RoutedEventArgs e)
    {
        var mineral = await LoadMineralFile();
        if (mineral != null) ViewModel?.LoadMineral2(mineral);
    }

    private async Task<MineralParams?> LoadMineralFile()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Mineral File",
            FileTypeFilter = new[] { new FilePickerFileType("Mineral File") { Patterns = new[] { "*.mine" } } },
            AllowMultiple = false,
        });

        if (files.Count == 1)
        {
            var json = await File.ReadAllTextAsync(files[0].Path.LocalPath, Encoding.UTF8);
            if (MineralParams.ImportJson(json, out var mineral))
            {
                return mineral;
            }
        }
        return null;
    }

    private async void OnSaveVProfile(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || ViewModel == null) return;

        var vpc = ViewModel.CreateVProfileCalculator();
        if (vpc == null)
        {
            ViewModel.StatusMessage = "Please load both minerals and generate ratios first.";
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save VProfile",
            SuggestedFileName = "profile.vpf",
            FileTypeChoices = new[] { new FilePickerFileType("VProfile File") { Patterns = new[] { "*.vpf" } } },
        });

        if (file != null)
        {
            await File.WriteAllTextAsync(file.Path.LocalPath, vpc.ExportJson(), Encoding.UTF8);
        }
    }

    private async void OnLoadVProfile(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || ViewModel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open VProfile",
            FileTypeFilter = new[] { new FilePickerFileType("VProfile File") { Patterns = new[] { "*.vpf" } } },
            AllowMultiple = false,
        });

        if (files.Count == 1)
        {
            var json = await File.ReadAllTextAsync(files[0].Path.LocalPath, Encoding.UTF8);
            if (VProfileCalculator.ImportJson(json, out var vpc) && vpc != null)
            {
                ViewModel.LoadFromVProfile(vpc);
            }
        }
    }

    private async void OnExportCsv(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || ViewModel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export CSV",
            SuggestedFileName = "mixture_results.csv",
            FileTypeChoices = new[] { new FilePickerFileType("CSV File") { Patterns = new[] { "*.csv" } } },
        });

        if (file != null)
        {
            MineralCsvIO.ExportResults(file.Path.LocalPath, ViewModel.Results.ToList());
        }
    }
}
