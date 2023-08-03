using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ZModLauncher.Attached_Properties;

public class BasicMenuButton : Button
{
    // Using a DependencyProperty as the backing store for HasHoverAnimationProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HasHoverAnimationProperty =
        DependencyProperty.Register("HasHoverAnimation", typeof(bool), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for TextMarginProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TextMarginProperty =
        DependencyProperty.Register("TextMargin", typeof(Thickness), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for CornerRadiusProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for PopupTextProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PopupTextProperty =
        DependencyProperty.Register("PopupText", typeof(string), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for HasPopupProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HasPopupProperty =
        DependencyProperty.Register("HasPopup", typeof(bool), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register("Icon", typeof(PathGeometry), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for ImageIcon.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ImageIconProperty =
        DependencyProperty.Register("ImageIcon", typeof(ImageSource), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for IconHeight.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IconHeightProperty =
        DependencyProperty.Register("IconHeight", typeof(double), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for IconWidth.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IconWidthProperty =
        DependencyProperty.Register("IconWidth", typeof(double), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for IconFill.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IconFillProperty =
        DependencyProperty.Register("IconFill", typeof(Brush), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for IconFillOnHover.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IconFillOnHoverProperty =
        DependencyProperty.Register("IconFillOnHover", typeof(Brush), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for IconBackground.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IconBackgroundProperty =
        DependencyProperty.Register("IconBackground", typeof(Brush), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for IconBackgroundHover.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IconBackgroundHoverProperty =
        DependencyProperty.Register("IconBackgroundHover", typeof(Brush), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for IconStrokeColor.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IconStrokeColorProperty =
        DependencyProperty.Register("IconStrokeColor", typeof(Brush), typeof(BasicMenuButton));

    // Using a DependencyProperty as the backing store for IconStrokeThickness.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IconStrokeThicknessProperty =
        DependencyProperty.Register("IconStrokeThickness", typeof(double), typeof(BasicMenuButton));

    public bool HasHoverAnimation
    {
        get => (bool)GetValue(HasHoverAnimationProperty);
        set => SetValue(HasHoverAnimationProperty, value);
    }

    public Thickness TextMargin
    {
        get => (Thickness)GetValue(TextMarginProperty);
        set => SetValue(TextMarginProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public string PopupText
    {
        get => (string)GetValue(PopupTextProperty);
        set => SetValue(PopupTextProperty, value);
    }

    public bool HasPopup
    {
        get => (bool)GetValue(HasPopupProperty);
        set => SetValue(HasPopupProperty, value);
    }

    public PathGeometry Icon
    {
        get => (PathGeometry)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ImageSource ImageIcon
    {
        get => (ImageSource)GetValue(ImageIconProperty);
        set => SetValue(ImageIconProperty, value);
    }

    public double IconHeight
    {
        get => (double)GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }

    public double IconWidth
    {
        get => (double)GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }

    public Brush IconFill
    {
        get => (Brush)GetValue(IconFillProperty);
        set => SetValue(IconFillProperty, value);
    }

    public Brush IconFillOnHover
    {
        get => (Brush)GetValue(IconFillOnHoverProperty);
        set => SetValue(IconFillOnHoverProperty, value);
    }

    public Brush IconBackground
    {
        get => (Brush)GetValue(IconBackgroundProperty);
        set => SetValue(IconBackgroundProperty, value);
    }

    public Brush IconBackgroundHover
    {
        get => (Brush)GetValue(IconBackgroundHoverProperty);
        set => SetValue(IconBackgroundHoverProperty, value);
    }

    public Brush IconStrokeColor
    {
        get => (Brush)GetValue(IconStrokeColorProperty);
        set => SetValue(IconStrokeColorProperty, value);
    }

    public double IconStrokeThickness
    {
        get => (double)GetValue(IconStrokeThicknessProperty);
        set => SetValue(IconStrokeThicknessProperty, value);
    }
}