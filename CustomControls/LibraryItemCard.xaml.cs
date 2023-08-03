using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZModLauncher.Attached_Properties;

namespace ZModLauncher.CustomControls;

/// <summary>
///     Interaction logic for LibraryItemCard.xaml
/// </summary>
public partial class LibraryItemCard : UserControl
{
    // Using a DependencyProperty as the backing store for ImageOpacity.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ImageOpacityProperty =
        DependencyProperty.Register("ImageOpacity", typeof(double), typeof(ImageBrush));

    // Using a DependencyProperty as the backing store for OptionsButton.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty OptionsButtonProperty =
        DependencyProperty.Register("OptionsButton", typeof(ComboBox), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for TrafficLightColor.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TrafficLightColorProperty =
        DependencyProperty.Register("TrafficLightColor", typeof(Brush), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for TrafficLight.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TrafficLightProperty =
        DependencyProperty.Register("TrafficLight", typeof(BasicMenuButton), typeof(LibraryItemCard));

    // Using a DependencyProperty as the backing store for FavoriteItemButton.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FavoriteItemButtonProperty =
        DependencyProperty.Register("FavoriteItemButton", typeof(StackPanel), typeof(LibraryItemCard));

    // Using a DependencyProperty as the backing store for FavoriteItemButton.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FavoriteItemButtonIndicatorProperty =
        DependencyProperty.Register("FavoriteItemButtonIndicator", typeof(BasicMenuButton), typeof(LibraryItemCard));

    // Using a DependencyProperty as the backing store for TrafficLightVisibility.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TrafficLightVisibilityProperty =
        DependencyProperty.Register("TrafficLightVisibility", typeof(Visibility), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(LibraryItemCard));

    // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(LibraryItemCard));

    // Using a DependencyProperty as the backing store for Status.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
        "Status", typeof(string), typeof(TextBlock));

    // Using a DependencyProperty as the backing store for ItemIsFavorited.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ItemIsFavoritedProperty = DependencyProperty.Register(
        "ItemIsFavorited", typeof(bool), typeof(LibraryItemCard));

    // Using a DependencyProperty as the backing store for Tags.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TagsProperty = DependencyProperty.Register(
        "Tags", typeof(List<string>), typeof(LibraryItemCard));

    public LibraryItemCard()
    {
        InitializeComponent();
    }

    public ImageSource ImageSource
    {
        get => (ImageSource)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Status
    {
        get => (string)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public Brush TrafficLightColor
    {
        get => (Brush)GetValue(TrafficLightColorProperty);
        set => SetValue(TrafficLightColorProperty, value);
    }

    public Visibility TrafficLightVisibility
    {
        get => (Visibility)GetValue(TrafficLightVisibilityProperty);
        set => SetValue(TrafficLightVisibilityProperty, value);
    }

    public BasicMenuButton TrafficLight
    {
        get => (BasicMenuButton)GetValue(TrafficLightProperty);
        set => SetValue(TrafficLightProperty, value);
    }

    public StackPanel FavoriteItemButton
    {
        get => (StackPanel)GetValue(FavoriteItemButtonProperty);
        set => SetValue(FavoriteItemButtonProperty, value);
    }

    public ComboBox OptionsButton
    {
        get => (ComboBox)GetValue(OptionsButtonProperty);
        set => SetValue(OptionsButtonProperty, value);
    }

    public double ImageOpacity
    {
        get => (double)GetValue(ImageOpacityProperty);
        set => SetValue(ImageOpacityProperty, value);
    }

    public bool ItemIsFavorited
    {
        get => (bool)GetValue(ItemIsFavoritedProperty);
        set => SetValue(ItemIsFavoritedProperty, value);
    }

    public List<string> Tags
    {
        get => (List<string>)GetValue(TagsProperty);
        set => SetValue(TagsProperty, value);
    }

    public BasicMenuButton FavoriteItemButtonIndicator
    {
        get => (BasicMenuButton)GetValue(FavoriteItemButtonIndicatorProperty);
        set => SetValue(FavoriteItemButtonIndicatorProperty, value);
    }

    public LibraryItemCard Clone()
    {
        return new LibraryItemCard
        {
            ImageSource = ImageSource,
            Title = Title,
            Status = Status,
            TrafficLightVisibility = TrafficLightVisibility,
            TrafficLight = TrafficLight,
            FavoriteItemButton = FavoriteItemButton,
            ItemIsFavorited = ItemIsFavorited
        };
    }

    private void This_Initialized(object sender, EventArgs e)
    {
        TrafficLightVisibility = Visibility.Collapsed;
    }

    private void TrafficLight_Initialized(object sender, EventArgs e)
    {
        TrafficLight = (BasicMenuButton)sender;
    }

    private void OptionsButton_Initialized(object sender, EventArgs e)
    {
        OptionsButton = (ComboBox)sender;
    }

    private void FavoriteItemButton_Initialized(object sender, EventArgs e)
    {
        FavoriteItemButton = (StackPanel)sender;
    }

    private void FavoriteItemButtonIndicator_Initialized(object sender, EventArgs e)
    {
        FavoriteItemButtonIndicator = (BasicMenuButton)sender;
    }
}