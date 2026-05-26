using Prism.Mvvm;

namespace AiSuite.ViewModels.Tools
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModelBrowserViewModel : BindableBase, IToolViewModel
    {
        private string modelDirectoryPath;

        public string DisplayName { get; } = "Model Browser";

        public string ModelDirectoryPath
        {
            get => modelDirectoryPath;
            set => SetProperty(ref modelDirectoryPath, value);
        }
    }
}