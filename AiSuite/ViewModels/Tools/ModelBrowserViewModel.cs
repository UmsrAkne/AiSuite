using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AiSuite.Databases;
using AiSuite.Models;
using AiSuite.Models.DTOs;
using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;

namespace AiSuite.ViewModels.Tools
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModelBrowserViewModel : BindableBase, IToolViewModel
    {
        private readonly MyDbContext dbContext;
        private readonly string thumbnailCacheDir;
        private string modelDirectoryPath;
        private AsyncRelayCommand loadImagesCommand;
        private AsyncRelayCommand searchModelCommand;
        private string searchText;

        public ModelBrowserViewModel(MyDbContext dbContext)
        {
            this.dbContext = dbContext;
            thumbnailCacheDir = Path.Combine(AppContext.BaseDirectory, "Thumbnails");
            if (!Directory.Exists(thumbnailCacheDir))
            {
                Directory.CreateDirectory(thumbnailCacheDir);
            }

            Images = new ObservableCollection<ModelFileItem>();
            ModelFileItemView = CollectionViewSource.GetDefaultView(Images);
        }

        public string DisplayName { get; } = "Model Browser";

        public string SearchText { get => searchText; set => SetProperty(ref searchText, value); }

        public string ModelDirectoryPath
        {
            get => modelDirectoryPath;
            set => SetProperty(ref modelDirectoryPath, value);
        }

        public ObservableCollection<ModelFileItem> Images { get; }

        public ICollectionView ModelFileItemView { get; set; }

        public AsyncRelayCommand SearchModelAsyncCommand =>
            searchModelCommand ??= new AsyncRelayCommand(async () =>
            {
                if (Images.Count == 0)
                {
                    return;
                }

                await Application.Current.Dispatcher.InvokeAsync(() => Search(searchText));
            });

        public AsyncRelayCommand LoadImagesAsyncCommand =>
        loadImagesCommand ??= new AsyncRelayCommand(async () =>
        {
            if (string.IsNullOrWhiteSpace(ModelDirectoryPath))
            {
                return;
            }

            await LoadImagesAsync(ModelDirectoryPath);
            await dbContext.AddRangeAsync(Images);
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
                    // DBから既存のキャッシュパスがあるか確認
                    var loraModel = dbContext.LoraModels.FirstOrDefault(m => m.ModelFilePath == item.FilePath);
                    var cachePath = loraModel?.ThumbnailPath;

                    BitmapSource bitmap;
                    if (!string.IsNullOrEmpty(cachePath) && File.Exists(cachePath))
                    {
                        // キャッシュがあればキャッシュからロード
                        bitmap = LoadThumbnail(cachePath, 150);
                    }
                    else
                    {
                        // キャッシュがない場合、元の画像から作成
                        var previewPath = item.GetPreviewImagePath();
                        if (File.Exists(previewPath))
                        {
                            bitmap = LoadThumbnail(previewPath, 150);
                            
                            // 作成したサムネイルを保存
                            var fileName = $"{Guid.NewGuid()}.png";
                            var savePath = Path.Combine(thumbnailCacheDir, fileName);
                            SaveBitmapSourceAsPng(bitmap, savePath);
                            item.ThumbnailCachePath = savePath;

                            // DBを更新
                            if (loraModel != null)
                            {
                                loraModel.ThumbnailPath = savePath;
                                await dbContext.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            bitmap = CreateEmptyBitmap(150, 200);
                        }
                    }

                    var metadata = Utils.ModelMetadataParser.ParseJsonFile<ModelMetadataDto>(item.CivitaiInfoPath);
                    item.ModelMetadataDto = metadata;

                    var helperInfoMetadata = Utils.ModelMetadataParser.ParseJsonFile<CivitaiHelperInfoDto>(item.CivitaiHelperInfoPath);
                    item.CivitaiHelperInfoDto = helperInfoMetadata;

                    // UIスレッドに通知して反映
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        item.Thumbnail = bitmap;
                    });

                    // 必要に応じてわずかなウェイトを入れるとUIがより滑らかになります
                    await Task.Delay(1).ConfigureAwait(false);
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

        private void SaveBitmapSourceAsPng(BitmapSource bitmapSource, string path)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            using var stream = File.Create(path);
            encoder.Save(stream);
        }

        private void Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                ModelFileItemView.Filter = null;
            }
            else
            {
                // ヒットしたやつだけ表示するようにフィルタ
                ModelFileItemView.Filter = (obj) =>
                {
                    var data = obj as ModelFileItem;
                    return data != null
                           && (data.ModelMetadataDto.Model.Name.Contains(keyword)
                               || data.ModelMetadataDto.Model.Description.Contains(keyword));
                };
            }
        }
    }
}