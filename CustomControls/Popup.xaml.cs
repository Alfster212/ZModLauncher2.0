using System.Windows;
using System.Windows.Controls;

namespace ZModLauncher.CustomControls;

/// <summary>
///     Interaction logic for Popup.xaml
/// </summary>
public partial class Popup : UserControl
{
    // Using a DependencyProperty as the backing store for HorizontalOffsetProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HorizontalOffsetProperty =
        DependencyProperty.Register("HorizontalOffset", typeof(int), typeof(Popup));

    // Using a DependencyProperty as the backing store for IsPopupOpenProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsPopupOpenProperty =
        DependencyProperty.Register("IsPopupOpen", typeof(bool), typeof(Popup));

    // Using a DependencyProperty as the backing store for PopupPlacementProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PopupPlacementProperty =
        DependencyProperty.Register("PopupPlacement", typeof(string), typeof(Popup));

    // Using a DependencyProperty as the backing store for PopupTextProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PopupTextProperty =
        DependencyProperty.Register("PopupText", typeof(string), typeof(Popup));

    public Popup()
    {
        InitializeComponent();
    }

    public int HorizontalOffset
    {
        get => (int)GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    public bool IsPopupOpen
    {
        get => (bool)GetValue(IsPopupOpenProperty);
        set => SetValue(IsPopupOpenProperty, value);
    }

    public string PopupPlacement
    {
        get => (string)GetValue(PopupPlacementProperty);
        set => SetValue(PopupPlacementProperty, value);
    }

    public string PopupText
    {
        get => (string)GetValue(PopupTextProperty);
        set => SetValue(PopupTextProperty, value);
    }
}