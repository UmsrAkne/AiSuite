using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AiSuite.Utils;
using Microsoft.Xaml.Behaviors;

namespace AiSuite.Behaviors
{
    public class MouseSelectionBehavior : Behavior<Canvas>
    {
        public readonly static DependencyProperty SelectionRectProperty =
            DependencyProperty.Register(
                nameof(SelectionRect),
                typeof(Rect),
                typeof(MouseSelectionBehavior),
                new FrameworkPropertyMetadata(
                    default(Rect),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public readonly static DependencyProperty ScaleProperty =
            DependencyProperty.Register(
                nameof(Scale),
                typeof(double),
                typeof(MouseSelectionBehavior),
                new PropertyMetadata(1.0));

        public readonly static DependencyProperty ImageWidthProperty =
            DependencyProperty.Register(
                nameof(ImageWidth),
                typeof(double),
                typeof(MouseSelectionBehavior),
                new PropertyMetadata(double.MaxValue));

        public readonly static DependencyProperty ImageHeightProperty =
            DependencyProperty.Register(
                nameof(ImageHeight),
                typeof(double),
                typeof(MouseSelectionBehavior),
                new PropertyMetadata(double.MaxValue));

        public readonly static DependencyProperty ConfirmSelectionCommandProperty =
            DependencyProperty.Register(
                nameof(ConfirmSelectionCommand),
                typeof(ICommand),
                typeof(MouseSelectionBehavior),
                new PropertyMetadata(null));

        // 選択範囲を外部（ViewModel）から参照・バインドするための依存関係プロパティ
        private Point startPoint;

        public ICommand ConfirmSelectionCommand
        {
            get => (ICommand)GetValue(ConfirmSelectionCommandProperty);
            set => SetValue(ConfirmSelectionCommandProperty, value);
        }

        public Rect SelectionRect
        {
            get => (Rect)GetValue(SelectionRectProperty);
            set => SetValue(SelectionRectProperty, value);
        }

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public double ImageWidth
        {
            get => (double)GetValue(ImageWidthProperty);
            set => SetValue(ImageWidthProperty, value);
        }

        public double ImageHeight
        {
            get => (double)GetValue(ImageHeightProperty);
            set => SetValue(ImageHeightProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += OnMouseDown;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseUp += OnMouseUp;
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= OnMouseDown;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseUp -= OnMouseUp;
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var rawPoint = e.GetPosition(AssociatedObject);
            startPoint = new Point(rawPoint.X, rawPoint.Y);
            AssociatedObject.CaptureMouse();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!AssociatedObject.IsMouseCaptured)
            {
                return;
            }

            var rawPoint = e.GetPosition(AssociatedObject);

            // 1. まず現在のマウス位置から、素の矩形情報を作る
            // 画像の範囲内に制限
            var clampedRawX = Math.Max(0, Math.Min(ImageWidth, rawPoint.X));
            var clampedRawY = Math.Max(0, Math.Min(ImageHeight, rawPoint.Y));
            var clampedStartX = Math.Max(0, Math.Min(ImageWidth, startPoint.X));
            var clampedStartY = Math.Max(0, Math.Min(ImageHeight, startPoint.Y));

            var rawX = Math.Min(clampedStartX, clampedRawX);
            var rawY = Math.Min(clampedStartY, clampedRawY);
            var rawWidth = Math.Abs(clampedStartX - clampedRawX);
            var rawHeight = Math.Abs(clampedStartY - clampedRawY);

            // 2. 座標を 8 の倍数に「切り捨て」
            // (x / 8) を整数にしてから 8 を掛ける
            var snappedX = Math.Floor(rawX / 8.0) * 8.0;
            var snappedY = Math.Floor(rawY / 8.0) * 8.0;

            // 3. サイズを 8 の倍数に「切り上げ」
            // (width / 8) を切り上げてから 8 を掛ける
            var snappedWidth = Math.Ceiling(rawWidth / 8.0) * 8.0;
            var snappedHeight = Math.Ceiling(rawHeight / 8.0) * 8.0;

            // 4. 画像の範囲内（ImageWidth, ImageHeight）に収まるように調整
            // まずは矩形の右端と下端を制限
            if (snappedX + snappedWidth > ImageWidth)
            {
                snappedWidth = Math.Floor((ImageWidth - snappedX) / 8.0) * 8.0;
            }

            if (snappedY + snappedHeight > ImageHeight)
            {
                snappedHeight = Math.Floor((ImageHeight - snappedY) / 8.0) * 8.0;
            }

            // 幅や高さが負にならないように（画像サイズより開始位置が大きい場合など）
            snappedWidth = Math.Max(0, snappedWidth);
            snappedHeight = Math.Max(0, snappedHeight);

            SelectionRect = new Rect(snappedX, snappedY, snappedWidth, snappedHeight);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject.IsMouseCaptured)
            {
                AssociatedObject.ReleaseMouseCapture();
                Logger.Log($"Selection confirmed: {SelectionRect}");
                ConfirmSelectionCommand?.Execute(null);
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && AssociatedObject.IsMouseCaptured)
            {
                AssociatedObject.ReleaseMouseCapture();
                SelectionRect = default;
                e.Handled = true;
                Logger.Log("Selection cancelled during drag.");
            }
        }
    }
}