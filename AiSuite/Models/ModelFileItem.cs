using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AiSuite.Models.DTOs;
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

        public ModelMetadataDto ModelMetadataDto { get; set; }

        /// <summary>
        /// 入力されたファイルパスの拡張子部分を存在する画像ファイルの拡張子に置き換えたパスを返す。
        /// 主に ".safetensors" を対象に実行する。
        /// </summary>
        /// <returns>置き換え処理後のパス。</returns>
        public string GetPreviewImagePath()
        {
            var modelFilePath = FilePath;
            var pathWithoutExtension = Path.GetFileNameWithoutExtension(modelFilePath);
            var baseDirectory = Path.GetDirectoryName(modelFilePath) ?? string.Empty;

            var png = Path.Combine(baseDirectory, $"{pathWithoutExtension}.preview.png").ToLower();
            var jpg = Path.Combine(baseDirectory, $"{pathWithoutExtension}.preview.jpg").ToLower();
            var jpeg = Path.Combine(baseDirectory, $"{pathWithoutExtension}.preview.jpeg").ToLower();
            var gif = Path.Combine(baseDirectory, $"{pathWithoutExtension}.preview.gif").ToLower();

            foreach (var imageFileName in new[] { png, jpg, jpeg, gif, })
            {
                var p = Path.Combine(baseDirectory, imageFileName);
                if (File.Exists(p))
                {
                    return p;
                }
            }

            // 全部見つからなかった場合はフォールバックで png を返す。
            return Path.Combine(baseDirectory, $"{pathWithoutExtension}.preview.png");
        }

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