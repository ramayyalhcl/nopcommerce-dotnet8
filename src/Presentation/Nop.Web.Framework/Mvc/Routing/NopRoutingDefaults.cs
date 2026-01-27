namespace Nop.Web.Framework.Mvc.Routing
{
    /// <summary>
    /// Routing defaults for nopCommerce
    /// </summary>
    public static partial class NopRoutingDefaults
    {
        /// <summary>
        /// Gets the route value key
        /// </summary>
        public static partial class RouteValue
        {
            /// <summary>
            /// Language route value key
            /// </summary>
            public static string Language => "language";
        }

        /// <summary>
        /// Gets the language parameter transformer name
        /// </summary>
        public static string LanguageParameterTransformer => "lang";
    }
}
