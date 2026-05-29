using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;

namespace AiSuite.Models
{
    public class ModelFileItem : BindableBase
    {
        private BitmapSource thumbnail;
        private string filePath;

        public ModelFileItem()
        {
            OpenCivitaiInfoCommand = new AsyncRelayCommand(
                async () => await OpenCivitaiInfoAsync(),
                () => !string.IsNullOrEmpty(CivitaiInfoPath));
        }

        public string FilePath
        {
            get => filePath;
            set
            {
                if (SetProperty(ref filePath, value))
                {
                    RaisePropertyChanged(nameof(FileName));
                    RaisePropertyChanged(nameof(FileNameWithExtension));
                }

                var dir = Path.GetDirectoryName(value) ?? string.Empty;
                var path = Path.Combine(dir, $"{FileNameWithExtension}.civitai.info");
                if (File.Exists(path))
                {
                    CivitaiInfoPath = path;
                }
            }
        }

        public string FileName => Path.GetFileName(FilePath);

        public string FileNameWithExtension => Path.GetFileNameWithoutExtension(FilePath);

        public BitmapSource Thumbnail { get => thumbnail; set => SetProperty(ref thumbnail, value); }

        public string CivitaiInfoPath { get; set; } = string.Empty;

        public AsyncRelayCommand OpenCivitaiInfoCommand { get; set; }

        private async Task OpenCivitaiInfoAsync()
        {
            if (string.IsNullOrEmpty(CivitaiInfoPath))
            {
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "notepad.exe",
                        Arguments = $"\"{CivitaiInfoPath}\"",
                        UseShellExecute = false,
                    });
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine($"失敗: {ex.Message}");
                }
            });
        }
    }
}