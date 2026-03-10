using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ThermoElastic.Core.Models;
using ThermoElastic.Desktop.ViewModels;

namespace ThermoElastic.Desktop.Views;

public partial class MineralEditorView : UserControl
{
    public MineralEditorView()
    {
        InitializeComponent();
    }

    private MineralEditorViewModel? ViewModel => DataContext as MineralEditorViewModel;

    private async void OnLoadMine(object? sender, RoutedEventArgs e)
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
                ViewModel?.FromMineralParams(mineral);
            }
        }
    }

    private async void OnSaveMine(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || ViewModel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Mineral File",
            SuggestedFileName = $"{ViewModel.MineralName}.mine",
            FileTypeChoices = new[] { new FilePickerFileType("Mineral File") { Patterns = new[] { "*.mine" } } },
        });

        if (file != null)
        {
            var mineral = ViewModel.ToMineralParams();
            await File.WriteAllTextAsync(file.Path.LocalPath, mineral.ExportJson(), Encoding.UTF8);
        }
    }

    private async void OnImportCsv(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import CSV",
            FileTypeFilter = new[] { new FilePickerFileType("CSV File") { Patterns = new[] { "*.csv" } } },
            AllowMultiple = false,
        });

        if (files.Count == 1)
        {
            var minerals = MineralParams.ImportCsvFile(files[0].Path.LocalPath);
            if (minerals.Count > 0)
            {
                ViewModel?.FromMineralParams(minerals[0]);

                var basePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ThermoElasticCalculator", "Minerals");
                Directory.CreateDirectory(basePath);

                foreach (var m in minerals)
                {
                    var filePath = Path.Combine(basePath, $"{m.MineralName}.mine");
                    await File.WriteAllTextAsync(filePath, m.ExportJson(), Encoding.UTF8);
                }

                if (ViewModel != null)
                    ViewModel.StatusMessage = $"Imported {minerals.Count} minerals.";
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
            SuggestedFileName = "minerals.csv",
            FileTypeChoices = new[] { new FilePickerFileType("CSV File") { Patterns = new[] { "*.csv" } } },
        });

        if (file != null)
        {
            var mineral = ViewModel.ToMineralParams();
            MineralParams.ExportCsvFile(file.Path.LocalPath, new List<MineralParams> { mineral });
        }
    }
}
