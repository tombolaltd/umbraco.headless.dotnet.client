using System;
using Newtonsoft.Json;
using Umbraco.Client.Config;

namespace Umbraco.Client.Queries
{
    class GetContentRegardlessOfPublishedStatus : IUmbracoCustomQuery<RawContentResponse>
    {
        public GetContentRegardlessOfPublishedStatus(int id)
        {
            AssociatedContentId = id;
        }

        public Guid UniqueQueryId { get; } = new Guid("EAD9950E-434D-441C-8E69-6E89BFE1D140");
        public int AssociatedContentId { get; }
        public RawContentResponse EmptyResponse => RawContentResponse.Empty;

        public string RelativeRequestUrl()
        {
            return string.Format(ApiPath.ContentIdWithTemplate, AssociatedContentId);
        }

        public RawContentResponse MapResponse(string jsonResponse)
        {
            return JsonConvert.DeserializeObject<RawContentResponse>(jsonResponse) ?? RawContentResponse.Empty;
        }
    }
}
