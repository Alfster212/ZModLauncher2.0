using System;
using System.Windows;
using System.Windows.Controls;

namespace ZModLauncher.Helpers;

/// <summary>
///     Assists with controlling various user interface functions and behaviors.
/// </summary>
public static class UIHelper
{
    /// <summary>
    ///     The main window frame used for rendering content within the application.
    /// </summary>
    private static readonly Frame _mainFrame = ((MainWindow)Application.Current.MainWindow).MainFrame;

    /// <summary>
    ///     Shows a user interface control.
    /// </summary>
    /// <param name="control"></param>
    public static void Show(UIElement control)
    {
        control.Visibility = Visibility.Visible;
    }

    /// <summary>
    ///     Hides a user interface control.
    /// </summary>
    /// <param name="control"></param>
    public static void Collapse(UIElement control)
    {
        control.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    ///     Enables interactivity for a user interface control.
    /// </summary>
    /// <param name="control"></param>
    public static void Enable(UIElement control)
    {
        control.IsEnabled = true;
    }

    /// <summary>
    ///     Disables interactivity for a user interface control.
    /// </summary>
    /// <param name="control"></param>
    public static void Disable(UIElement control)
    {
        control.IsEnabled = false;
    }

    /// <summary>
    ///     Displays an error dialog with a specified message and button mode.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="buttonMode"></param>
    /// <returns></returns>
    public static MessageBoxResult ShowErrorDialog(string message, MessageBoxButton buttonMode = MessageBoxButton.OK)
    {
        return MessageBox.Show(message, "Error", buttonMode, MessageBoxImage.Error);
    }

    /// <summary>
    ///     Displays an information dialog with a specified message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static MessageBoxResult ShowInformationDialog(string message)
    {
        return MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    ///     Displays an interrogative dialog with a specified message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static MessageBoxResult ShowQuestionDialog(string message)
    {
        return MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
    }

    /// <summary>
    ///     Checks to see if the previously visited page's name matches the specified name.
    /// </summary>
    /// <param name="pageName"></param>
    /// <returns></returns>
    public static bool IsPreviousPage(string pageName)
    {
        return _mainFrame.Content != null && _mainFrame.Content.ToString().Contains(pageName);
    }

    /// <summary>
    ///     Checks to see if the current page's name matches the specified name.
    /// </summary>
    /// <param name="pageName"></param>
    /// <returns></returns>
    public static bool IsCurrentPage(string pageName)
    {
        return _mainFrame.CurrentSource.ToString().Contains(pageName);
    }

    /// <summary>
    ///     Attempts to navigate to a page which matches the specified name.
    /// </summary>
    /// <param name="pageName"></param>
    public static void NavigateToPage(string pageName)
    {
        _mainFrame.Navigate(new Uri($"../Pages/{pageName}.xaml", UriKind.Relative));
    }

    /// <summary>
    ///     Navigates back to the previous page in the stack from the current page.
    /// </summary>
    public static void GoBackFromCurrentPage()
    {
        _mainFrame.GoBack();
    }
}