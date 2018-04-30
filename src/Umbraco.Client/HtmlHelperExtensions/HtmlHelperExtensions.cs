using System.Web.Mvc;

namespace Umbraco.Client.HtmlHelperExtensions
{
    /// <summary>
    /// Tombola Umbraco client extensions
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Creates a new umbraco request scope
        /// </summary>
        public static UmbracoClientRequest Cms(this HtmlHelper helper)
        {
            return UmbracoConnectionHolder.Instance.NewRequest();
        }
    }
}
