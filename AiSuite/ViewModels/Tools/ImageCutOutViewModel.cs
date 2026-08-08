using Prism.Mvvm;

namespace AiSuite.ViewModels.Tools
{
    public class ImageCutOutViewModel : BindableBase, IToolViewModel
    {
        private string imagePath;

        public string DisplayName => "Image Cut Out";

        public string ImagePath { get => imagePath; set => SetProperty(ref imagePath, value); }
    }
}