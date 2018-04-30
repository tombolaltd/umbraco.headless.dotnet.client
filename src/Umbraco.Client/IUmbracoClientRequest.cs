using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Umbraco.Client
{
    internal interface IUmbracoClientRequest
    {
        IHtmlString GetContentRegardlessOfPublishedStatus(int id);
        Task<IHtmlString> GetContentRegardlessOfPublishedStatusAsync(int id);

        IHtmlString GetPublishedContent(int id);
        IHtmlString GetPublishedContent(int id, bool bypassCache);
        IHtmlString GetPublishedContent(string url);
        IHtmlString GetPublishedContent(string url, bool bypassCache);
        Task<IHtmlString> GetPublishedContentAsync(int id);
        Task<IHtmlString> GetPublishedContentAsync(int id, bool bypassCache);
        Task<IHtmlString> GetPublishedContentAsync(string url);
        Task<IHtmlString> GetPublishedContentAsync(string url, bool bypassCache);

        IHtmlString GetItemWithSpecifiedTemplate(int id, int templateid, bool bypassCache);
        Task<IHtmlString> GetItemWithSpecifiedTemplateAsync(int id, int templateid, bool bypassCache);
        IHtmlString GetItemWithSpecifiedTemplate(string url, int templateid, bool bypassCache);
        Task<IHtmlString> GetItemWithSpecifiedTemplateAsync(string url, int templateid, bool bypassCache);

        RawContentResponse GetPublishedContentIncludingMetadata(int id);
        RawContentResponse GetPublishedContentIncludingMetadata(int id, bool bypassCache);
        RawContentResponse GetPublishedContentIncludingMetadata(string url);
        RawContentResponse GetPublishedContentIncludingMetadata(string url, bool bypassCache);
        Task<RawContentResponse> GetPublishedContentIncludingMetadataAsync(int id);
        Task<RawContentResponse> GetPublishedContentIncludingMetadataAsync(int id, bool bypassCache);
        Task<RawContentResponse> GetPublishedContentIncludingMetadataAsync(string url);
        Task<RawContentResponse> GetPublishedContentIncludingMetadataAsync(string url, bool bypassCache);

        IDictionary<string, RawContentResponse> GetPublishedDescendantsOfFolder(int id);
        IDictionary<string, RawContentResponse> GetPublishedDescendantsOfFolder(int id, bool bypassCache);
        IDictionary<string, RawContentResponse> GetPublishedDescendantsOfFolder(string url);
        IDictionary<string, RawContentResponse> GetPublishedDescendantsOfFolder(string url, bool bypassCache);
        Task<IDictionary<string, RawContentResponse>> GetPublishedDescendantsOfFolderAsync(int id);
        Task<IDictionary<string, RawContentResponse>> GetPublishedDescendantsOfFolderAsync(int id, bool bypassCache);
        Task<IDictionary<string, RawContentResponse>> GetPublishedDescendantsOfFolderAsync(string url);
        Task<IDictionary<string, RawContentResponse>> GetPublishedDescendantsOfFolderAsync(string url, bool bypassCache);

        IDictionary<string, RawContentResponse> GetPublishedContentOfTreePicker(int id, string property);
        IDictionary<string, RawContentResponse> GetPublishedContentOfTreePicker(int id, string property, bool bypassCache);
        Task<IDictionary<string, RawContentResponse>> GetPublishedContentOfTreePickerAsync(int id, string property);
        Task<IDictionary<string, RawContentResponse>> GetPublishedContentOfTreePickerAsync(int id, string property, bool bypassCache);

        Task<TResponse> QueryAsync<TResponse>(IUmbracoCustomQuery<TResponse> query);
        Task<TResponse> QueryAsync<TResponse>(IUmbracoCustomQuery<TResponse> query, bool bypassCache);
    }


    public interface IUmbracoCustomQuery<out TOut>
    {
        Guid UniqueQueryId { get; }
        int AssociatedContentId { get; }
        TOut EmptyResponse { get; }
        string RelativeRequestUrl();
        TOut MapResponse(string jsonResponse);
    }
}