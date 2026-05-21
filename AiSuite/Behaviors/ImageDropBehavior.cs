#nullable enable
using System.Linq;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace AiSuite.Behaviors
{
    public class ImageDropBehavior : Behavior<FrameworkElement>
    {
        public readonly static DependencyProperty ImagePathProperty =
            DependencyProperty.Register(
                nameof(ImagePath),
                typeof(string),
                typeof(ImageDropBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string? ImagePath
        {
            get => (string?)GetValue(ImagePathProperty);
            set => SetValue(ImagePathProperty, value);
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
                var files = (string[] ?)e.Data.GetData(DataFormats.FileDrop);
                var imageFile = files?.FirstOrDefault(f =>
                    f.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)
                    || f.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase)
                    || f.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase)
                    || f.EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase));

                if (imageFile != null)
                {
                    ImagePath = imageFile;
                }
            }
        }
    }
}