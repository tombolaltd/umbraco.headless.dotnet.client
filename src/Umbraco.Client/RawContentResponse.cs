using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Umbraco.Client
{
    [Serializable]
    public class RawContentResponse
    {

        /// <summary>
        /// The Content ID generated by umbraco.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name given to the content. Usually equivalent to a page name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The rendered markup of the content as a raw string.
        /// </summary>
        public string RenderedContent { get; set; } = string.Empty;

        /// <summary>
        /// The rendered markup of the content as a decoded, HTML ready string.
        /// </summary>
        public IHtmlString RenderedContentHtmlString => new HtmlString(HttpUtility.HtmlDecode(RenderedContent));

        /// <summary>
        /// Represents an empty content response. 
        /// Useful as a more explicit way of doing new RawContentResponse();
        /// </summary>
        public static RawContentResponse Empty => new RawContentResponse();

        /// <summary>
        /// Contains property values as a key value pair.
        /// </summary>
        public ICollection<KeyValuePair<string, string>> PropertyCollection { get; set; }

        /// <summary>
        /// content specific meta tags exposed as key value  pair collection, key = name, value = content
        /// </summary>
        public ICollection<KeyValuePair<string, string>> MetaTagCollection { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// Exposed popular meta tags to be used in the tenant
        /// </summary>
        /// <returns></returns>
        public string MetaTagPageTitle() => MetaTagCollection.Where(x => x.Key == "title").SingleOrDefault().Value ?? string.Empty;
        public string MetaTagPageDescription() => MetaTagCollection.Where(x => x.Key == "description").SingleOrDefault().Value ?? string.Empty;

        /// <summary>
        /// html meta tags as string
        /// </summary>
        public string RenderedMetaTags { get; set; } = string.Empty;

        /// <summary>
        /// should this content be visible to players logged in
        /// </summary>
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool VisibleLoggedIn { get; set; }

        /// <summary>
        /// should this content be visible to players logged out
        /// </summary>
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool VisibleLoggedOut { get; set; }

        /// <summary>
        /// content meta tags as a html string for injection into the head tag of documents where consumed
        /// </summary>
        public IHtmlString RenderedMetaTagsHtmlString => new HtmlString(RenderedMetaTags);


        /// <summary>
        /// Date the content last updated.
        /// </summary>
        public DateTime UpdateDate { get; set; }
    }
}