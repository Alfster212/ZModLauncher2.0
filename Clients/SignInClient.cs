using System;
using System.Threading.Tasks;
using ZModLauncher.Constants;
using ZModLauncher.Core;
using ZModLauncher.Pages;
using static ZModLauncher.Helpers.UIHelper;
using static ZModLauncher.Constants.GlobalStringConstants;

namespace ZModLauncher.Clients;

/// <summary>
///     Represents a generic client for sign-in operations.
/// </summary>
public abstract class SignInClient : OAuthConstants
{
    /// <summary>
    ///     The current browser page used for rendering the user interface used to sign-in.
    /// </summary>
    public readonly SignInBrowser BrowserPage;

    /// <summary>
    ///     Creates a new sign-in client with a specified owning browser page.
    /// </summary>
    /// <param name="browserPage"></param>
    protected SignInClient(SignInBrowser browserPage)
    {
        BrowserPage = browserPage;
        BrowserPage.browser.NavigationCompleted += (_, _) => { CheckUserMembership(); };
    }

    /// <summary>
    ///     Represents a generic handler for checking a user's membership status to a particular platform.
    /// </summary>
    public abstract void CheckUserMembership();

    /// <summary>
    ///     Retrieves the currently set URL address from the owning browser page.
    /// </summary>
    /// <returns></returns>
    public string GetBrowserUrl()
    {
        return BrowserPage.browser.Source.ToString();
    }

    /// <summary>
    ///     Sets the URL address in the owning browser page to the specified URL.
    /// </summary>
    /// <param name="url"></param>
    public void SetBrowserUrl(string url)
    {
        BrowserPage.browser.Source = new Uri(url);
    }

    /// <summary>
    ///     Executes a specified JavaScript DOM code snippet within the owning browser page.
    /// </summary>
    /// <param name="domAction"></param>
    /// <returns></returns>
    public async Task<string> ExecuteDOMAction(string domAction)
    {
        return await new JavaScriptDOMAction
        {
            Action = domAction,
            Client = this
        }.Execute();
    }

    /// <summary>
    ///     Checks to see if the currently set browser URL contains the specified string value.
    /// </summary>
    /// <param name="targetString"></param>
    /// <returns></returns>
    private bool BrowserUrlContainsString(string targetString)
    {
        return GetBrowserUrl().IndexOf(targetString, StringComparison.Ordinal) != -1;
    }

    /// <summary>
    ///     Checks to see if the sign-in redirect page is the current page.
    /// </summary>
    /// <returns></returns>
    public bool IsOnRedirectPage()
    {
        return GetBrowserUrl().StartsWith(RedirectUri);
    }

    /// <summary>
    ///     Checks to see if the login page used for sign-in operations is the current page.
    /// </summary>
    /// <returns></returns>
    public bool IsOnLoginPage()
    {
        return BrowserUrlContainsString("login");
    }

    /// <summary>
    ///     Checks to see if the intermediate authorization page is the current page.
    /// </summary>
    /// <returns></returns>
    public bool IsOnAuthorizePage()
    {
        return BrowserUrlContainsString("authorize");
    }

    /// <summary>
    ///     Checks to see if the client has successfully established a connection to the internet.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsConnectedToInternet()
    {
        string result = await ExecuteDOMAction("Array.from(document.querySelectorAll('div'))\r\n.find(el => el.textContent.includes(\"You're not connected\"))");
        return result.IndexOf(NoInternetResponse, StringComparison.Ordinal) == -1;
    }

    /// <summary>
    ///     Sends the end-user back to the sign-in page used for launching the browser page.
    /// </summary>
    public void SendBackToSignInPage()
    {
        NavigateToPage(SignInPageName);
    }
}