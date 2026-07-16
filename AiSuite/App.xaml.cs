using System.Windows;
using AiSuite.Databases;
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
            containerRegistry.Register<IToolViewModel, ModelBrowserViewModel>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // DIコンテナから MyDbContext を取り出して EnsureCreated を実行する
            using var context = Container.Resolve<MyDbContext>();

            #if DEBUG
            // デバッグ起動時のみ、毎回DBをリセットして初期化する
            context.Database.EnsureDeleted();
            #endif

            // context.Database.Migrate();
            context.Database.EnsureCreated();
        }
    }
}