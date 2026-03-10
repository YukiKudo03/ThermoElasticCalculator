using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
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
}
