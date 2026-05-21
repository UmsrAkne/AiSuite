using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using AiSuite.Utils;
using Prism.Mvvm;

namespace AiSuite.ViewModels;

public class MainWindowViewModel : BindableBase
{
    #if DEBUG
    // ReSharper disable once UnusedMember.Local
    private readonly string testDirectoryPath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            @"myFiles\tests\RiderProjects\AiSuite");
    #endif

    private readonly AppVersionInfo appVersionInfo = new();

    public MainWindowViewModel(IEnumerable<IToolViewModel> tools)
    {
        Tools = new ObservableCollection<IToolViewModel>(tools);
        SetupDummyData();
    }

    public string Title => appVersionInfo.Title;

    public ObservableCollection<IToolViewModel> Tools { get; }

    [Conditional("DEBUG")]
    private void SetupDummyData()
    {
    }
}