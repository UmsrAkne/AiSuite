#nullable enable
using System.Linq;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace AiSuite.Behaviors
{
    public class JsonFileDropBehavior : Behavior<FrameworkElement>
    {
        public readonly static DependencyProperty JsonFilePathProperty =
            DependencyProperty.Register(
                nameof(JsonFilePath),
                typeof(string),
                typeof(JsonFileDropBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string? JsonFilePath
        {
            get => (string?)GetValue(JsonFilePathProperty);
            set => SetValue(JsonFilePathProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AllowDrop = true;
            AssociatedObject.PreviewDragOver += OnPreviewDragOver;
            AssociatedObject.Drop += OnDrop;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewDragOver -= OnPreviewDragOver;
            AssociatedObject.Drop -= OnDrop;
        }

        private void OnPreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
                var jsonFile = files?.FirstOrDefault(f =>
                    f.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase));

                if (jsonFile != null)
                {
                    JsonFilePath = jsonFile;
                }
            }
        }
    }
}