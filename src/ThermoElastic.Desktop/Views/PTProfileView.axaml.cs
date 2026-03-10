using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ThermoElastic.Core.IO;
using ThermoElastic.Core.Models;
using ThermoElastic.Desktop.ViewModels;

namespace ThermoElastic.Desktop.Views;

public partial class PTProfileView : UserControl
{
    public PTProfileView()
    {
        InitializeComponent();
    }

    private PTProfileViewModel? ViewModel => DataContext as PTProfileViewModel;

    private async void OnLoadMineral(object? sender, RoutedEventArgs e)
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
                ViewModel?.LoadMineral(mineral);
            }
        }
    }

    private async void OnLoadProfile(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || ViewModel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open PT Profile",
            FileTypeFilter = new[] { new FilePickerFileType("PT Profile") { Patterns = new[] { "*.ptpf" } } },
            AllowMultiple = false,
        });

        if (files.Count == 1)
        {
            var json = await File.ReadAllTextAsync(files[0].Path.LocalPath, Encoding.UTF8);
            if (PTProfile.ImportJson(json, out var profile) && profile != null)
            {
                ViewModel.PTDataList.Clear();
                foreach (var pt in profile.Profile)
                {
                    ViewModel.PTDataList.Add(pt);
                }
            }
        }
    }

    private async void OnSaveProfile(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || ViewModel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save PT Profile",
            SuggestedFileName = "profile.ptpf",
            FileTypeChoices = new[] { new FilePickerFileType("PT Profile") { Patterns = new[] { "*.ptpf" } } },
        });

        if (file != null)
        {
            var profile = new PTProfile { Name = "Profile", Profile = ViewModel.PTDataList.ToList() };
            await File.WriteAllTextAsync(file.Path.LocalPath, profile.ExportJson(), Encoding.UTF8);
        }
    }

    private async void OnExportCsv(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || ViewModel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export CSV",
            SuggestedFileName = "results.csv",
            FileTypeChoices = new[] { new FilePickerFileType("CSV File") { Patterns = new[] { "*.csv" } } },
        });

        if (file != null)
        {
            MineralCsvIO.ExportResults(file.Path.LocalPath, ViewModel.Results.ToList());
        }
    }
}
