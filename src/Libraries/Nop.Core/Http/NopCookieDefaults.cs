namespace Nop.Core.Http
{
    public static partial class NopCookieDefaults
    {
        public static string Prefix => "nop.";
        public static string CustomerCookie => "customer";
        public static string CompareProductsCookie => "compareproducts";
        public static string RecentlyViewedProductsCookie => "recentlyviewedproducts";
        public static string AntiforgeryCookie => "antiforgery";
        public static string SessionCookie => "session";
        public static string IgnoreEuCookieLawWarning => "ignoreEuCookieLawWarning";
    }
}
