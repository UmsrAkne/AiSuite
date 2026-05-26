using Prism.Mvvm;

namespace AiSuite.ViewModels.Tools
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModelBrowserViewModel : BindableBase, IToolViewModel
    {
        public string DisplayName { get; } = "Model Browser";
    }
}