using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace HubLink.Services;

public interface IFileDialogService
{
    Task<string?> PickCsvSavePathAsync(string suggestedFileName);
}

public sealed class AvaloniaFileDialogService : IFileDialogService
{
    private readonly Window _owner;

    public AvaloniaFileDialogService(Window owner)
    {
        _owner = owner;
    }

    public async Task<string?> PickCsvSavePathAsync(string suggestedFileName)
    {
        var storageProvider = _owner.StorageProvider;
        if (!storageProvider.CanSave)
        {
            return null;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Shipments to CSV",
            SuggestedFileName = suggestedFileName,
            DefaultExtension = "csv",
            FileTypeChoices =
            [
                new FilePickerFileType("CSV Files") { Patterns = ["*.csv"] }
            ]
        });

        return file?.TryGetLocalPath();
    }
}
