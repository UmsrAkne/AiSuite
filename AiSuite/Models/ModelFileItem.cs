using System.IO;
using System.Windows.Media.Imaging;
using Prism.Mvvm;

namespace AiSuite.Models
{
    public class ModelFileItem : BindableBase
    {
        private BitmapSource thumbnail;

        public string FilePath { get; set; }

        public string FileName => Path.GetFileName(FilePath);

        public string FIleNameWithExtension => Path.GetFileNameWithoutExtension(FilePath);

        public BitmapSource Thumbnail { get => thumbnail; set => SetProperty(ref thumbnail, value); }
    }
}