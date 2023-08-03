using System.Threading.Tasks;
using ZModLauncher.Clients;

namespace ZModLauncher.Core;

/// <summary>
///     Represents a JavaScript code snippet intended to be executed within an HTML DOM tree.
/// </summary>
public class JavaScriptDOMAction
{
    /// <summary>
    ///     The code snippet represented as a string value.
    /// </summary>
    public string Action;
    /// <summary>
    ///     The sign-in client specific to the current instance of the DOM action object.
    /// </summary>
    public SignInClient Client;

    /// <summary>
    ///     Executes the currently set DOM action code snippet.
    /// </summary>
    /// <returns></returns>
    public async Task<string> Execute()
    {
        return await Client.BrowserPage.browser.ExecuteScriptAsync(Action);
    }
}