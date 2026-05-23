#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace AiSuite.Behaviors
{
    public class ImageFilesDropBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty ImagePathsProperty =
            DependencyProperty.Register(
                nameof(ImagePaths),
                typeof(ICollection<string>),
                typeof(ImageFilesDropBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ICollection<string>? ImagePaths
        {
            get => (ICollection<string>?)GetValue(ImagePathsProperty);
            set => SetValue(ImagePathsProperty, value);
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
                if (files == null) return;

                var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", };
                var imageFiles = files.Where(f =>
                    imageExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (imageFiles.Any() && ImagePaths != null)
                {
                    foreach (var file in imageFiles)
                    {
                        if (!ImagePaths.Contains(file))
                        {
                            ImagePaths.Add(file);
                        }
                    }
                }
            }
        }
    }
}