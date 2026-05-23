using System.Windows;
using AiSuite.ViewModels;
using AiSuite.ViewModels.Tools;
using AiSuite.Views;
using Prism.Ioc;

namespace AiSuite
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IToolViewModel, RectMeasureViewModel>();
            containerRegistry.Register<IToolViewModel, PromptBatcherViewModel>();
        }
    }
}