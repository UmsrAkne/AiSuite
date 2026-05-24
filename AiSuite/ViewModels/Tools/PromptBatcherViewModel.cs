using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Mvvm;

namespace AiSuite.ViewModels.Tools
{
    public class PromptBatcherViewModel : BindableBase, IToolViewModel
    {
        private string newNodeTitle = string.Empty;

        public PromptBatcherViewModel()
        {
            AddNodeTitleCommand = new DelegateCommand(AddNodeTitle);
            RemoveNodeTitleCommand = new DelegateCommand<string>(RemoveNodeTitle);
        }

        public string DisplayName { get; } = "Prompt Batcher";

        public ObservableCollection<string> ImagePaths { get; set; } = new ();

        public ObservableCollection<string> NodeTitles { get; set; } = new ()
        {
            "Positive Prompt Node",
            "Negative Prompt Node",
        };

        public string NewNodeTitle { get => newNodeTitle; set => SetProperty(ref newNodeTitle, value); }

        public DelegateCommand AddNodeTitleCommand { get; }

        public DelegateCommand<string> RemoveNodeTitleCommand { get; }

        private void AddNodeTitle()
        {
            if (string.IsNullOrWhiteSpace(NewNodeTitle))
            {
                return;
            }

            NodeTitles.Add(NewNodeTitle);
            NewNodeTitle = string.Empty;
        }

        private void RemoveNodeTitle(string title)
        {
            if (NodeTitles.Contains(title))
            {
                NodeTitles.Remove(title);
            }
        }
    }
}