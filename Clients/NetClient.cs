using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ZModLauncher.Clients;

/// <summary>
///     Represents a generic client used for performing network related operations.
/// </summary>
public class NetClient
{
    /// <summary>
    ///     The client used to perform network operations through the HTTP service protocol.
    /// </summary>
    public static readonly HttpClient Client = new();
    /// <summary>
    ///     The content to be sent within an HTTP request created through the client.
    /// </summary>
    public string RequestContent;
    /// <summary>
    ///     The type of data being used within the request content.
    /// </summary>
    public string RequestFormat;
    /// <summary>
    ///     The URL address to send an HTTP request to.
    /// </summary>
    public string Url;

    /// <summary>
    ///     Creates a new client with no header name or value.
    /// </summary>
    public NetClient() { }

    /// <summary>
    ///     Creates a new client with the specified header name and value.
    /// </summary>
    /// <param name="headerName"></param>
    /// <param name="headerValue"></param>
    public NetClient(string headerName, string headerValue)
    {
        Client.DefaultRequestHeaders.Remove(headerName);
        Client.DefaultRequestHeaders.Add(headerName, headerValue);
    }

    /// <summary>
    ///     Serializes a returned HTTP response object into a UTF-8 string representation.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    private static async Task<string> SerializeString(HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    ///     Serializes a returned HTTP response object into the specified data-type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="response"></param>
    /// <returns></returns>
    private static async Task<T> Serialize<T>(HttpResponseMessage response)
    {
        return JsonConvert.DeserializeObject<T>(await SerializeString(response));
    }

    /// <summary>
    ///     Sends an HTTP GET request to the currently set URL address.
    /// </summary>
    /// <returns></returns>
    public async Task<HttpResponseMessage> GET()
    {
        return await Client.GetAsync(Url);
    }

    /// <summary>
    ///     Sends an HTTP POST request to the currently set URL address.
    /// </summary>
    /// <returns></returns>
    public async Task<HttpResponseMessage> POST()
    {
        StringContent content = new(RequestContent, Encoding.UTF8, RequestFormat);
        return await Client.PostAsync(Url, content);
    }

    /// <summary>
    ///     Sends an HTTP GET request to the currently set URL address and serializes the response.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<T> GET<T>()
    {
        return await Serialize<T>(await GET());
    }

    /// <summary>
    ///     Sends an HTTP POST request to the currently set URL address and serializes the response.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<T> POST<T>()
    {
        return await Serialize<T>(await POST());
    }
}