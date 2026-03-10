using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ThermoElastic.Core.IO;
using ThermoElastic.Core.Models;
using ThermoElastic.Desktop.ViewModels;

namespace ThermoElastic.Desktop.Views;

public partial class RockCalculatorView : UserControl
{
    public RockCalculatorView()
    {
        InitializeComponent();
    }

    private RockCalculatorViewModel? ViewModel => DataContext as RockCalculatorViewModel;

    private async void OnAddMineral(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Mineral File",
            FileTypeFilter = new[] { new FilePickerFileType("Mineral File") { Patterns = new[] { "*.mine" } } },
            AllowMultiple = false,
        });

        if (files.Count == 1)
        {
            var json = await File.ReadAllTextAsync(files[0].Path.LocalPath, Encoding.UTF8);
            if (MineralParams.ImportJson(json, out var mineral) && mineral != null)
            {
                ViewModel?.AddMineral(mineral);
            }
        }
    }

    private async void OnLoadRock(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || ViewModel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Rock File",
            FileTypeFilter = new[] { new FilePickerFileType("Rock File") { Patterns = new[] { "*.rock" } } },
            AllowMultiple = false,
        });

        if (files.Count == 1)
        {
            var json = await File.ReadAllTextAsync(files[0].Path.LocalPath, Encoding.UTF8);
            if (RockComposition.ImportJson(json, out var rock) && rock != null)
            {
                ViewModel.FromRockComposition(rock);
            }
        }
    }

    private async void OnSaveRock(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || ViewModel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Rock File",
            SuggestedFileName = $"{ViewModel.RockName}.rock",
            FileTypeChoices = new[] { new FilePickerFileType("Rock File") { Patterns = new[] { "*.rock" } } },
        });

        if (file != null)
        {
            var rock = ViewModel.ToRockComposition();
            await File.WriteAllTextAsync(file.Path.LocalPath, rock.ExportJson(), Encoding.UTF8);
        }
    }

    private async void OnExportCsv(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || ViewModel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export CSV",
            SuggestedFileName = "rock_results.csv",
            FileTypeChoices = new[] { new FilePickerFileType("CSV File") { Patterns = new[] { "*.csv" } } },
        });

        if (file != null)
        {
            MineralCsvIO.ExportResults(file.Path.LocalPath, ViewModel.Results.ToList());
        }
    }
}
