using System.Windows;
using System.Windows.Media.Animation;

namespace ZModLauncher.Helpers;

/// <summary>
/// Contains various methods for controlling user interface animations.
/// </summary>
public static class AnimationHelper
{
    /// <summary>
    /// Applies the specified storyboard animation to the specified UI control.
    /// </summary>
    /// <param name="anim"></param>
    /// <param name="control"></param>
    public static void ApplyStoryboardAnim(DependencyObject anim, DependencyObject control)
    {
        if (anim != null) Storyboard.SetTarget(anim, control);
    }

    /// <summary>
    /// Plays the specified storyboard animation for all UI controls which are linked to it.
    /// </summary>
    /// <param name="anim"></param>
    public static void Play(Storyboard anim)
    {
        anim.Begin();
    }

    /// <summary>
    /// Stops the specified storyboard animation for all UI controls which are linked to it.
    /// </summary>
    /// <param name="anim"></param>
    public static void Stop(Storyboard anim)
    {
        anim.Stop();
    }
}