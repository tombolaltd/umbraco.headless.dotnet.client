using System;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Client.Config;

namespace Umbraco.Client
{
    internal class UmbracoAlternateTemplateQuery : IUmbracoCustomQuery<IHtmlString>
    {

        private const string baseGUID = "C4A0987D-1376-4F6E-9BD9-0143FDEEC389";

        public UmbracoAlternateTemplateQuery(int associatedContentId, int templateId)
        {
            AssociatedContentId = associatedContentId;
            TemplateID = templateId;
        }

        public UmbracoAlternateTemplateQuery(string url, int templateId, Func<string, RawContentResponse> mappingQuery)
        {
            URL = url;
            TemplateID = templateId;
            AssociatedContentId = mappingQuery(url).Id;
        }

        public Func<string, RawContentResponse> RawContentQuery { set; get; }
        
      
        public string URL { get; set; }

        public int TemplateID { get; set; }

        public int AssociatedContentId { get; set; }
        public IHtmlString EmptyResponse => new HtmlString(string.Empty);

        public Guid UniqueQueryId
        {
            get
            {
                var contentId = string.Join(string.Empty, AssociatedContentId.ToString(), TemplateID.ToString());
                var itemGuid = string.Join(string.Empty, baseGUID.Substring(0, baseGUID.Length - contentId.Length),
                    contentId);
                return new Guid(itemGuid);
            }
        }

        public IHtmlString MapResponse(string jsonResponse)
        {
            RawContentResponse rawContentResponse = JsonConvert.DeserializeObject<RawContentResponse>(jsonResponse);
            return new HtmlString(HttpUtility.HtmlDecode(rawContentResponse.RenderedContent));
        }

        public string RelativeRequestUrl()
        {
            return string.Format(ApiPath.PublishedContentIdSpecifiedTemplate,$"{AssociatedContentId}/{TemplateID}");
        }
    }
}