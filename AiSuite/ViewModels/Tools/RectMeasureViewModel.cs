#nullable enable
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using AiSuite.Models;
using AiSuite.Utils;
using Prism.Commands;
using Prism.Mvvm;

namespace AiSuite.ViewModels.Tools;

// ReSharper disable once ClassNeverInstantiated.Global
public class RectMeasureViewModel : BindableBase, IToolViewModel
{
    private readonly AppVersionInfo appVersionInfo = new();
    private readonly DispatcherTimer historyDebounceTimer;
    private Rect selectionArea;
    private double scale = 0.5;
    private string? imagePath;
    private int pixelWidth;
    private int pixelHeight;
    private (HistoryType Type, string Description, string Detail)? pendingHistory;

    public RectMeasureViewModel()
    {
        historyDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000),
        };
        historyDebounceTimer.Tick += HistoryDebounceTimer_Tick;

        CopyCommand = new DelegateCommand(CopySelectionToClipboard);
        ResetSelectionCommand = new DelegateCommand(ResetSelection);
        ReloadCommand = new DelegateCommand(ReloadImage);
        ConfirmSelectionCommand = new DelegateCommand(ConfirmSelection);
        SetupDummyData();
    }

    public DelegateCommand CopyCommand { get; }

    public DelegateCommand ResetSelectionCommand { get; }

    public DelegateCommand ReloadCommand { get; }

    public DelegateCommand ConfirmSelectionCommand { get; }

    public ObservableCollection<HistoryItem> History { get; } = new();

    public string Title => appVersionInfo.Title;

    public string? ImagePath
    {
        get => imagePath;
        set
        {
            if (SetProperty(ref imagePath, value))
            {
                UpdateImageSize(value);
                AddHistory(HistoryType.ImageLoad, "Image Loaded", Path.GetFileName(value) ?? string.Empty);
            }
        }
    }

    public int PixelWidth
    {
        get => pixelWidth;
        set => SetProperty(ref pixelWidth, value);
    }

    public int PixelHeight
    {
        get => pixelHeight;
        set => SetProperty(ref pixelHeight, value);
    }

    public Rect SelectionArea
    {
        get => selectionArea;
        set
        {
            if (SetProperty(ref selectionArea, value))
            {
                RaiseScreenPropertiesChanged();
            }
        }
    }

    public double Scale
    {
        get => scale;
        set
        {
            if (SetProperty(ref scale, value))
            {
                RaiseScreenPropertiesChanged();
            }
        }
    }

    public double ScreenX
    {
        get => selectionArea.X;
        set
        {
            if (Math.Abs(selectionArea.X - value) < 0.1)
            {
                return;
            }

            // 直接 X を書き換えるのではなく、Rect ごと作り直して代入する
            var rect = selectionArea;
            rect.X = value;
            SelectionArea = rect;
            AddHistoryDebounced(HistoryType.SelectionConfirmed, "X Modified", $"{value:F0}");
        }
    }

    public double ScreenY
    {
        get => selectionArea.Y;
        set
        {
            if (Math.Abs(selectionArea.Y - value) < 0.1)
            {
                return;
            }

            var rect = selectionArea;
            rect.Y = value;
            SelectionArea = rect;
            AddHistoryDebounced(HistoryType.SelectionConfirmed, "Y Modified", $"{value:F0}");
        }
    }

    public double ScreenWidth
    {
        get => selectionArea.Width;
        set
        {
            if (Math.Abs(selectionArea.Width - value) < 0.1)
            {
                return;
            }

            var rect = selectionArea;
            rect.Width = value;
            SelectionArea = rect;
            AddHistoryDebounced(HistoryType.SelectionConfirmed, "Width Modified", $"{value:F0}");
        }
    }

    public double ScreenHeight
    {
        get => selectionArea.Height;
        set
        {
            if (Math.Abs(selectionArea.Height - value) < 0.1)
            {
                return;
            }

            var rect = selectionArea;
            rect.Height = value;
            SelectionArea = rect;
            AddHistoryDebounced(HistoryType.SelectionConfirmed, "Height Modified", $"{value:F0}");
        }
    }

    public double[] AvailableScales { get; } = { 1.0, 0.5, 0.25, };

    public string DisplayName { get; } = "Rect Measure";

    private void ReloadImage()
    {
        if (string.IsNullOrEmpty(ImagePath))
        {
            return;
        }

        UpdateImageSize(ImagePath);
        AddHistory(HistoryType.ImageReload, "Image Reloaded", Path.GetFileName(ImagePath));
    }

    private void ConfirmSelection()
    {
        AddHistory(HistoryType.SelectionConfirmed, "Selection Confirmed", $"{selectionArea.X:F0}, {selectionArea.Y:F0}, {selectionArea.Width:F0}, {selectionArea.Height:F0}");
    }

    private void AddHistoryDebounced(HistoryType type, string description, string detail)
    {
        pendingHistory = (type, description, detail);
        historyDebounceTimer.Stop();
        historyDebounceTimer.Start();
    }

    private void HistoryDebounceTimer_Tick(object? sender, EventArgs e)
    {
        historyDebounceTimer.Stop();
        if (pendingHistory.HasValue)
        {
            AddHistory(pendingHistory.Value.Type, pendingHistory.Value.Description, pendingHistory.Value.Detail);
            pendingHistory = null;
        }
    }

    private void AddHistory(HistoryType type, string description, string detail)
    {
        History.Insert(0, new HistoryItem { Type = type, Description = description, Detail = detail });
        Logger.Log($"History Added: {description} - {detail}");
    }

    private void UpdateImageSize(string? path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            PixelWidth = 0;
            PixelHeight = 0;
            return;
        }

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var decoder = System.Windows.Media.Imaging.BitmapDecoder.Create(stream, System.Windows.Media.Imaging.BitmapCreateOptions.DelayCreation, System.Windows.Media.Imaging.BitmapCacheOption.None);
            var frame = decoder.Frames[0];
            PixelWidth = frame.PixelWidth;
            PixelHeight = frame.PixelHeight;
        }
        catch (Exception e)
        {
            Logger.Log($"Failed to get image size: {e.Message}");
            PixelWidth = 0;
            PixelHeight = 0;
        }
    }

    private void RaiseScreenPropertiesChanged()
    {
        RaisePropertyChanged(nameof(ScreenX));
        RaisePropertyChanged(nameof(ScreenY));
        RaisePropertyChanged(nameof(ScreenWidth));
        RaisePropertyChanged(nameof(ScreenHeight));
    }

    private void CopySelectionToClipboard()
    {
        var text = $"{selectionArea.X},{selectionArea.Y},{selectionArea.Width},{selectionArea.Height}";
        Clipboard.SetText(text);
        Logger.Log($"Copied to clipboard: {text}");
    }

    private void ResetSelection()
    {
        SelectionArea = default;
        Logger.Log("Selection reset.");
    }

    [Conditional("DEBUG")]
    private void SetupDummyData()
    {
    }
}