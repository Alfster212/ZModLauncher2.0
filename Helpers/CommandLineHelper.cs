using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static ZModLauncher.Helpers.UIHelper;
using static ZModLauncher.Constants.GlobalStringConstants;

namespace ZModLauncher.Helpers;

/// <summary>
/// Contains various methods for interacting with the internal Microsoft Windows command line.
/// </summary>
public static class CommandLineHelper
{
    /// <summary>
    /// Waits for the specified process to exit asynchronously before resuming execution in the launcher.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> WaitForExitAsync(Process process, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<int> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        void Process_Exited(object sender, EventArgs e)
        {
            tcs.TrySetResult(process.ExitCode);
        }

        try
        {
            process.EnableRaisingEvents = true;
        }
        catch (InvalidOperationException) when (process.HasExited) { }
        using (cancellationToken.Register(() => tcs.TrySetCanceled()))
        {
            process.Exited += Process_Exited;
            try
            {
                if (process.HasExited)
                {
                    tcs.TrySetResult(process.ExitCode);
                }
                return await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                process.Exited -= Process_Exited;
            }
        }
    }

    /// <summary>
    /// Attempts to launch an executable file with optional command line arguments and parameters.
    /// </summary>
    /// <param name="workingDir"></param>
    /// <param name="fileName"></param>
    /// <param name="args"></param>
    /// <param name="isSilent"></param>
    /// <param name="waitForExit"></param>
    /// <returns></returns>
    public static async Task LaunchExecutable(string workingDir, string fileName, string args = "", bool isSilent = false, bool waitForExit = false)
    {
        Process process = new();
        ProcessStartInfo processInfo = new()
        {
            UseShellExecute = isSilent,
            WorkingDirectory = workingDir,
            FileName = fileName,
            Arguments = args,
            WindowStyle = isSilent ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
        };
        process.StartInfo = processInfo;
        try
        {
            process.Start();
            if (waitForExit) await WaitForExitAsync(process);
        }
        catch
        {
            ShowErrorDialog(ModExecutableError);
        }
    }
}