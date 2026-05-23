using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace AiSuite.ViewModels.Tools
{
    public class PromptBatcherViewModel : BindableBase, IToolViewModel
    {
        public string DisplayName { get; } = "Prompt Batcher";

        public ObservableCollection<string> ImagePaths { get; set; } = new();
    }
}