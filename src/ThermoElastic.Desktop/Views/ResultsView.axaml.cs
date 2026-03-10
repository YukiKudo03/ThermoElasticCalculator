using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ThermoElastic.Core.IO;
using ThermoElastic.Desktop.ViewModels;

namespace ThermoElastic.Desktop.Views;

public partial class ResultsView : UserControl
{
    public ResultsView()
    {
        InitializeComponent();
    }

    private ResultsViewModel? ViewModel => DataContext as ResultsViewModel;

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
