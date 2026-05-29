using System.IO;
using System.Windows.Media.Imaging;
using Prism.Mvvm;

namespace AiSuite.Models
{
    public class ModelFileItem : BindableBase
    {
        private BitmapSource thumbnail;
        private string filePath;

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
    }
}