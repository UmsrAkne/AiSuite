using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
        private AsyncRelayCommand<ModelFileItem> openCivitaiInfoCommand;

        public string DisplayName { get; } = "Model Browser";

        public string ModelDirectoryPath
        {
            get => modelDirectoryPath;
            set => SetProperty(ref modelDirectoryPath, value);
        }

        public ObservableCollection<ModelFileItem> Images { get; } = new ();

        public AsyncRelayCommand LoadImagesAsyncCommand =>
        loadImagesCommand ??= new AsyncRelayCommand(async () =>
        {
            if (string.IsNullOrWhiteSpace(ModelDirectoryPath))
            {
                return;
            }

            await LoadImagesAsync(ModelDirectoryPath);
        });

        public AsyncRelayCommand<ModelFileItem> OpenCivitaiInfoCommand =>
            openCivitaiInfoCommand ??= new AsyncRelayCommand<ModelFileItem>(async (item) =>
            {
                if (item == null || string.IsNullOrEmpty(item.CivitaiInfoPath))
                {
                    return;
                }

                if (File.Exists(item.CivitaiInfoPath))
                {
                    await Task.Run(() =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = item.CivitaiInfoPath,
                            UseShellExecute = true,
                        });
                    });
                }
            });

        private async Task LoadImagesAsync(string folderPath)
        {
            Images.Clear();

            // .safetensors のファイルを全て取得する
            var allowedExtensions = new[] { ".safetensors", };
            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToLower()));

            var items = files.Select(f => new ModelFileItem { FilePath = f, }).ToList();
            foreach (var item in items)
            {
                Images.Add(item);
            }

            // バックグラウンドで画像を1枚ずつ非同期ロード（順次画面に描画される）
            await Task.Run(async () =>
            {
                foreach (var item in items)
                {
                    var bitmap = LoadThumbnail(GetPreviewImagePath(item.FilePath), 150); // 横幅を縮小

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
            const int defaultHeight = 200;

            try
            {
                if (!File.Exists(path))
                {
                    return CreateEmptyBitmap(decodeWidth, defaultHeight);
                }

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
                return CreateEmptyBitmap(decodeWidth, defaultHeight); // 壊れた画像などの対策
            }
        }

        private BitmapSource CreateEmptyBitmap(int width, int height)
        {
            var stride = width * 4;
            var pixels = new byte[stride * height]; // 初期値はすべて 0 (透明)

            var bitmap = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Pbgra32,
                null,
                pixels,
                stride);
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// 入力されたファイルパスの拡張子部分を ".preview.png" に置き換えたパスを返す。
        /// 主に ".safetensors" を対象に実行する。
        /// </summary>
        /// <param name="modelFilePath">処理対象のファイルパス。</param>
        /// <returns>置き換え処理後のパス。</returns>
        private string GetPreviewImagePath(string modelFilePath)
        {
            var pathWithoutExtension = Path.GetFileNameWithoutExtension(modelFilePath);
            var baseDirectory = Path.GetDirectoryName(modelFilePath) ?? string.Empty;
            return Path.Combine(baseDirectory, $"{pathWithoutExtension}.preview.png");
        }
    }
}