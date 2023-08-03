using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ZModLauncher.Helpers;

public class ThreadHelper
{
    /// <summary>
    ///     Executes an action scheduled to run in the background on the current thread pool.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task RunBackgroundAction(Action action)
    {
        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
    }

    /// <summary>
    ///     Executes an action scheduled to run in the background on the current thread pool.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task RunBackgroundAction(Func<Task> action)
    {
        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
    }
}