using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZModLauncher.Core;
using ZModLauncher.Managers;

namespace ZModLauncher.CustomControls;

/// <summary>
///     Interaction logic for LibraryItemCard.xaml
/// </summary>
public partial class AddLocalModDialog : UserControl
{
    // Using a DependencyProperty as the backing store for IsInEditMode.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsInEditModeProperty =
        DependencyProperty.Register("IsInEditMode", typeof(bool), typeof(AddLocalModDialog));

    private static Mod FocusedMod;

    public AddLocalModDialog()
    {
        InitializeComponent();
    }

    public bool IsInEditMode
    {
        get => (bool)GetValue(IsInEditModeProperty);
        set => SetValue(IsInEditModeProperty, value);
    }

    private void AddLocalModBackButton_OnClick(object sender, RoutedEventArgs e)
    {
        Visibility = Visibility.Collapsed;
    }

    private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        Keyboard.ClearFocus();
    }

    public void PopulateControlValuesFromMod(Mod mod)
    {
        nameBox.InputText = mod.Name;
        folderBox.InputText = mod.LocalPath;
        imageBox.InputText = mod.ImageUri;
        toggleMacroBox.IsChecked = mod.IsUsingSharedToggleMacro;
        mergeableFilesBox.InputText = string.Join(",", mod.MergeableFiles);
        shouldFavoriteBox.IsChecked = mod.IsFavorited;
        launchExeBox.InputText = mod.ExecutablePath;
        FocusedMod = mod;
    }

    private async Task ConfirmAddOrEditMod()
    {
        bool isEditedModLaunchable = IsInEditMode && FocusedMod.IsLaunchable || !IsInEditMode && !string.IsNullOrEmpty(launchExeBox.InputText);
        LocalModParams modParams = new()
        {
            Name = nameBox.InputText,
            FolderPath = folderBox.InputText,
            ImagePath = imageBox.InputText,
            UseSharedToggleMacro = toggleMacroBox.IsChecked,
            ShouldFavorite = shouldFavoriteBox.IsChecked,
            LaunchPath = launchExeBox.InputText
        };
        if (!Mod.AreLocalPropertiesValid(modParams, isEditedModLaunchable)) return;
        List<string> mergeableFiles = mergeableFilesBox.InputText.Split(',').ToList();
        modParams.MergeableFiles = mergeableFiles;
        Mod mod = IsInEditMode ? FocusedMod : Mod.CreateLocalMod(modParams);
        statusLabel.Visibility = Visibility.Visible;
        statusLabel.Text = IsInEditMode ? $"Applying edits to {mod.Name}..." : $"Installing {mod.Name}...";
        IsEnabled = false;
        confirmButton.IsEnabled = false;
        if (IsInEditMode) mod.Edit(modParams);
        else await mod.InstallLocal(modParams);
        await Task.Delay(1000);
        statusLabel.Text = IsInEditMode ? "Edits applied! Refreshing library..." : $"{mod.Name} is now installed! Refreshing library...";
        await Task.Delay(1000);
        statusLabel.Visibility = Visibility.Collapsed;
        Visibility = Visibility.Collapsed;
        IsEnabled = true;
        confirmButton.IsEnabled = true;
        await LibraryManager.RefreshLibrary();
    }

    private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        await ConfirmAddOrEditMod();
    }

    private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        dialogTitle.Content = IsInEditMode ? $"EDITING {FocusedMod.Name.ToUpper()}" : "ADD LOCAL MOD";
        folderBox.IsEnabled = !IsInEditMode;
        launchExeBox.IsEnabled = !IsInEditMode || FocusedMod.IsLaunchable;
    }
}