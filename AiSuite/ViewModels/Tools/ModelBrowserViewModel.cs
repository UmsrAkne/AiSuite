using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using AiSuite.Models;
using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;

namespace AiSuite.ViewModels.Tools
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModelBrowserViewModel : BindableBase, IToolViewModel
    {
        private string modelDirectoryPath;
        private AsyncRelayCommand loadImagesCommand;

        public string DisplayName { get; } = "Model Browser";

        public string ModelDirectoryPath
        {
            get => modelDirectoryPath;
            set => SetProperty(ref modelDirectoryPath, value);
        }

        public ObservableCollection<ImageItem> Images { get; } = new ();

        public AsyncRelayCommand LoadImagesAsyncCommand =>
        loadImagesCommand ??= new AsyncRelayCommand(async () =>
        {
            if (string.IsNullOrWhiteSpace(ModelDirectoryPath))
            {
                return;
            }

            await LoadImagesAsync(ModelDirectoryPath);
        });

        private async Task LoadImagesAsync(string folderPath)
        {
            Images.Clear();

            // 1. まずはファイルパスだけを超高速で取得してリストに並べる（画面には枠だけが出る）
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", };
            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToLower()));

            var items = files.Select(f => new ImageItem { FilePath = f, }).ToList();
            foreach (var item in items)
            {
                Images.Add(item);
            }

            // 2. バックグラウンドで画像を1枚ずつ非同期ロード（順次画面に描画される）
            // Task.Run でUIスレッドを絶対に止めない
            await Task.Run(async () =>
            {
                foreach (var item in items)
                {
                    var bitmap = LoadThumbnail(item.FilePath, 150); // 横幅を縮小

                    // UIスレッドに通知して反映
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        item.Thumbnail = bitmap;
                    });

                    // 必要に応じてわずかなウェイトを入れるとUIがより滑らかになります
                    await Task.Delay(1);
                }
            });
        }

        private BitmapSource LoadThumbnail(string path, int decodeWidth)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // ファイルロックを解除
                bitmap.CreateOptions = BitmapCreateOptions.DelayCreation;
                bitmap.DecodePixelWidth = decodeWidth; // 指定サイズに縮小デコードしてメモリを節約
                bitmap.UriSource = new Uri(path);
                bitmap.EndInit();
                bitmap.Freeze(); // スレッド間での共有を可能にする
                return bitmap;
            }
            catch
            {
                return null; // 壊れた画像などの対策
            }
        }
    }
}