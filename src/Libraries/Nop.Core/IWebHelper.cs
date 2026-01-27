namespace Nop.Core
{
    /// <summary>
    /// Represents a web helper
    /// </summary>
    public partial interface IWebHelper
    {
        string GetUrlReferrer();
        string GetCurrentIpAddress();
        string GetThisPageUrl(bool includeQueryString, bool? useSsl = null, bool lowercaseUrl = false);
        bool IsCurrentConnectionSecured();
        string GetStoreHost(bool useSsl);
        string GetStoreLocation(bool? useSsl = null);
        bool IsStaticResource();
        string ModifyQueryString(string url, string queryStringModification, string anchor);
        string RemoveQueryString(string url, string queryString);
        T QueryString<T>(string name);
        void RestartAppDomain(bool makeRedirect = false, string redirectUrl = "");
        bool IsRequestBeingRedirected { get; }
        bool IsPostBeingDone { get; set; }
    }
}
