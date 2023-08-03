using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
using ZModLauncher.Managers;
using ZModLauncher.Pages;
using static ZModLauncher.Helpers.StringHelper;
using static ZModLauncher.Helpers.UIHelper;
using static ZModLauncher.Constants.GlobalStringConstants;

namespace ZModLauncher.Clients;

/// <summary>
///     Represents a client for Patreon sign-in operations.
/// </summary>
public class PatreonSignInClient : SignInClient
{
    /// <summary>
    ///     The current browser page used for rendering the user interface used to sign-in.
    /// </summary>
    /// <param name="browserPage"></param>
    public PatreonSignInClient(SignInBrowser browserPage) : base(browserPage) { }

    /// <summary>
    ///     Performs a mouse click on the Patreon allow sign-in button in the browser if it exists.
    /// </summary>
    private async void ClickAllowButtonIfExists()
    {
        const string allowButtonDomAction = "document.getElementsByClassName(\"patreon-button patreon-button-action\")";
        string allowButtonDomElementName = await ExecuteDOMAction(allowButtonDomAction);
        if (allowButtonDomElementName != "{}") await ExecuteDOMAction($"{allowButtonDomAction}[0].click();");
    }

    /// <summary>
    ///     Retrieves a single-use access code to be used within the authorization process.
    /// </summary>
    private void GetSingleUseCode()
    {
        SingleUseCode = ExtractString(GetBrowserUrl(),
            "code=", "&state");
    }

    /// <summary>
    ///     Retrieves a user access token necessary for signing into Patreon.
    /// </summary>
    /// <returns></returns>
    private async Task GetUserAccessToken()
    {
        UserAccessToken = (await new NetClient
        {
            RequestContent = $"code={SingleUseCode}&grant_type=authorization_code&client_id="
                + $"{ClientId}&client_secret={ClientSecret}&redirect_uri={RedirectUri}",
            Url = TokenUrl,
            RequestFormat = "application/x-www-form-urlencoded"
        }.POST<JObject>()).GetValue("access_token")?.ToString();
    }

    /// <summary>
    ///     Checks to see if the end-user's membership to Patreon, should it exist, is valid.
    /// </summary>
    /// <returns></returns>
    private async Task<bool> IsMembershipValid()
    {
        LauncherConfigManager configManager = new();
        JObject membership = await new NetClient("Authorization", $"Bearer {UserAccessToken}")
        {
            Url =
                "https://www.patreon.com/api/oauth2/v2/identity?include=memberships.currently_entitled_tiers,memberships&fields%5Bmember%5D"
                + "=campaign_lifetime_support_cents,currently_entitled_amount_cents,email,full_name,is_follower,"
                + "last_charge_date,last_charge_status,lifetime_support_cents,next_charge_date,note,"
                + "patron_status,pledge_cadence,pledge_relationship_start,will_pay_amount_cents"
        }.GET<JObject>();
        try
        {
            string currentTierId = membership["included"]?[0]?["relationships"]?["currently_entitled_tiers"]?["data"]?[0]?["id"]?.ToString();
            if (currentTierId == configManager.LauncherConfig[RejectTierIdKey]?.ToString()) return false;
            string membershipStatus = membership["included"]?[0]?["attributes"]?["patron_status"]?.ToString();
            return membershipStatus is ActivePatronStatus or DeclinedPatronStatus;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Attempts to authorize the end-user's membership to complete the Patreon sign-in process.
    /// </summary>
    /// <returns></returns>
    private async Task AttemptToAuthorizeMembership()
    {
        if (await IsMembershipValid() && Application.Current.MainWindow != null)
            NavigateToPage(LoadingPageName);
        else
        {
            Process.Start(CreatorUrl);
            SendBackToSignInPage();
        }
    }

    /// <summary>
    ///     Attempts to validate the end-user's membership to Patreon, revoking an invalid membership.
    /// </summary>
    public override async void CheckUserMembership()
    {
        if (!await IsConnectedToInternet())
        {
            Show(BrowserPage.browser);
            await Task.Delay(2000);
            SendBackToSignInPage();
            return;
        }
        Collapse(BrowserPage.browser);
        ClickAllowButtonIfExists();
        if (IsOnRedirectPage())
        {
            GetSingleUseCode();
            await GetUserAccessToken();
            await AttemptToAuthorizeMembership();
        }
        else if (IsOnLoginPage()) Show(BrowserPage.browser);
        else if (!IsOnAuthorizePage()) SendBackToSignInPage();
    }
}