using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Polly;
using Umbraco.Caching;
using Umbraco.Caching.CacheValueTypes;
using Umbraco.Client.Config;
using Umbraco.Client.Connections;
using Umbraco.Client.EventArgs;
using Umbraco.Client.Queries;

namespace Umbraco.Client
{
    /// <summary>
    /// Represents a request to umbraco for content
    /// </summary>
    public class UmbracoClientRequest : IUmbracoClientRequest
    {
        private readonly HttpClient httpClient;
        private readonly ICacheProvider cache;
        private UmbracoRequestSettings settings = new UmbracoRequestSettings();
        private readonly bool cacheIsAvailable;

        /// <summary>
        /// Constructs a new umbraco client request
        /// </summary>
        public UmbracoClientRequest(Uri apiUrl, ICacheProvider cache, HttpClient httpClient)
            : this(new Target(apiUrl), cache, httpClient)
        {
        }

        /// <summary>
        /// Constructs a new umbraco client request
        /// </summary>
        public UmbracoClientRequest(Target target, ICacheProvider cache, HttpClient httpClient)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            this.cache = cache;
            this.cacheIsAvailable = cache != null;
            this.httpClient = httpClient;
            Target = target;
        }

        /// <summary>
        /// The umbraco endpoint, abstracted as a target.
        /// </summary>
        public Target Target { get; }

        /// <summary>
        /// A request event which fires whenever a http request to umbraco is retried.
        /// </summary>
        public event OnRequestFailureHandler OnRequestRetry;

        /// <summary>
        /// Requests content regardless of it's published status (gets BOTH published and unpublished content) from umbraco.
        /// This request bypasses the cache
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        public IHtmlString GetContentRegardlessOfPublishedStatus(int id)
        {
            return GetContentRegardlessOfPublishedStatusAsync(id).Result;
        }

        /// <summary>
        /// Requests content regardless of it's published status (gets BOTH published and unpublished content) from umbraco asynchronously.
        /// This request bypasses the cache
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        public async Task<IHtmlString> GetContentRegardlessOfPublishedStatusAsync(int id)
        {
            var query = new GetContentRegardlessOfPublishedStatus(id);
            var contentResponse = await QueryAsync(query, true).ConfigureAwait(false) ?? RawContentResponse.Empty;
            return contentResponse.RenderedContentHtmlString;
        }

        /// <summary>
        /// Requests published content from umbraco.
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        public IHtmlString GetPublishedContent(int id)
        {
            return GetPublishedContent(id, false);
        }

        /// <summary>
        /// Requests published content from umbraco. With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        /// <param name="bypassCache">If true - The request will bypass the cache, always going to the server to get the content</param>
        public IHtmlString GetPublishedContent(int id, bool bypassCache)
        {
            return GetPublishedContentAsync(id, bypassCache).Result;
        }

        /// <summary>
        /// Requests published content by url from umbraco.
        /// </summary>
        /// <param name="url">
        /// The unique ID of the content, which can be found in the umbraco back office
        /// example: index/hello-world
        /// </param>
        public IHtmlString GetPublishedContent(string url)
        {
            return GetPublishedContent(url, false);
        }

        /// <summary>
        /// Requests published content by url from umbraco. With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="url">
        /// The unique ID of the content, which can be found in the umbraco back office
        /// example: index/hello-world
        /// </param>
        /// <param name="bypassCache">If true - The request will bypass the cache, always going to the server to get the content</param>
        public IHtmlString GetPublishedContent(string url, bool bypassCache)
        {
            return GetPublishedContentAsync(url, bypassCache).Result;
        }

        /// <summary>
        /// Requests published content from umbraco asynchronously.
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        public Task<IHtmlString> GetPublishedContentAsync(int id)
        {
            return GetPublishedContentAsync(id, false);
        }

        /// <summary>
        /// Renders a content item from UMBRACO using a specified template id
        /// </summary>
        /// <param name="id">The content identifier</param>
        /// <param name="templateid">the template id</param>
        /// <param name="bypassCache">if true will always return content from the server</param>
        /// <returns></returns>
        public IHtmlString GetItemWithSpecifiedTemplate(int id, int templateid, bool bypassCache = false)
        {
            return GetItemWithSpecifiedTemplateAsync(id, templateid, bypassCache).Result;
        }

        /// <summary>
        /// Renders a content item from UMBRACO using a specified template id asyncronously
        /// </summary>
        /// <param name="QueryKey">unique Key for each content / template combination generate new guid for each call</param>
        /// <param name="id">The content identifier</param>
        /// <param name="templateid">the template id</param>
        /// <param name="bypassCache">if true will always return content from the server</param>
        public async Task<IHtmlString> GetItemWithSpecifiedTemplateAsync(int id, int templateid, bool bypassCache = false)
        {
            var altTempQ = new UmbracoAlternateTemplateQuery(associatedContentId: id, templateId: templateid);
            IHtmlString contentResponse = await QueryAsync(altTempQ, bypassCache).ConfigureAwait(false);
            
            return contentResponse;
        }
        /// <summary>
        /// Requests published content from umbraco asynchronously. With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="url">url to the content</param>
        /// <param name="templateid">the template id</param>
        /// <param name="bypassCache">if true will always return content from the server</param>
        /// <returns></returns>
        public IHtmlString GetItemWithSpecifiedTemplate(string url, int templateid,
            bool bypassCache = false)
        {
            return GetItemWithSpecifiedTemplateAsync(url, templateid, bypassCache).Result;
        }

        /// <summary>
        /// Requests published content from umbraco asynchronously. With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="url">url to the content</param>
        /// <param name="templateid">the template id</param>
        /// <param name="bypassCache">if true will always return content from the server</param>
        /// <returns></returns>
        public async Task<IHtmlString> GetItemWithSpecifiedTemplateAsync(string url, int templateid,
            bool bypassCache = false)
        {
            var altTempQ = new UmbracoAlternateTemplateQuery(url: url, templateId: templateid,
                mappingQuery: GetPublishedContentIncludingMetadata);
            IHtmlString contentResponse = await QueryAsync(altTempQ, bypassCache).ConfigureAwait(false);
            return contentResponse;
        }

        /// <summary>
        /// Requests published content from umbraco asynchronously. With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        /// <param name="bypassCache">If true - The request will bypass the cache, always going to the server to get the content</param>
        public async Task<IHtmlString> GetPublishedContentAsync(int id, bool bypassCache)
        {
            RawContentResponse contentResponse = await ExecuteContentRequestAsync(id, ApiPath.PublishedContentIdWithTemplate, bypassCache, ContentIdCacheSearch(), AddContentToCache()).ConfigureAwait(false);
            return contentResponse.RenderedContentHtmlString;
        }

        /// <summary>
        /// Requests published content by url from umbraco asynchronously.
        /// </summary>
        /// <param name="url">
        /// The unique ID of the content, which can be found in the umbraco back office
        /// example: index/hello-world
        /// </param>
        public Task<IHtmlString> GetPublishedContentAsync(string url)
        {
            return GetPublishedContentAsync(url, false);
        }

        /// <summary>
        /// Requests published content by url from umbraco asynchronously. With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="url">
        /// The unique ID of the content, which can be found in the umbraco back office
        /// example: index/hello-world
        /// </param>
        /// <param name="bypassCache">If true - The request will bypass the cache, always going to the server to get the content</param>
        public async Task<IHtmlString> GetPublishedContentAsync(string url, bool bypassCache)
        {
            string preparedUrl = PrepareUrlParameter(url);
            var result = await ExecuteContentRequestAsync(preparedUrl, ApiPath.PublishedContentByUrlWithTemplate, bypassCache, ContentUrlCacheSearch(), 
                                    AddContentAndUrlIndexToCache(preparedUrl)).ConfigureAwait(false);
            return result.RenderedContentHtmlString;
        }

        /// <summary>
        /// Requests published content by ID from umbraco, including content metadata - With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="id">
        /// The unique ID of the content, which can be found in the umbraco back office
        /// </param>
        public RawContentResponse GetPublishedContentIncludingMetadata(int id)
        {
            return GetPublishedContentIncludingMetadata(id, false);
        }

        /// <summary>
        /// Requests published content by ID from umbraco, including content metadata - With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="id">
        /// The unique ID of the content, which can be found in the umbraco back office
        /// </param>
        /// <param name="bypassCache">If true - The request will bypass the cache, always going to the server to get the content</param>
        public RawContentResponse GetPublishedContentIncludingMetadata(int id, bool bypassCache)
        {
            return GetPublishedContentIncludingMetadataAsync(id, bypassCache).Result;
        }

        /// <summary>
        /// Requests published content by url from umbraco, including content metadata - With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="url">
        /// The unique url of the content generated by umbraco based upon the content tree, which can be found in the umbraco back office
        /// example: index/hello-world
        /// </param>
        public RawContentResponse GetPublishedContentIncludingMetadata(string url)
        {
            return GetPublishedContentIncludingMetadata(url, false);
        }

        /// <summary>
        /// Requests published content by url from umbraco, including content metadata - With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="url">
        /// The unique url of the content generated by umbraco based upon the content tree, which can be found in the umbraco back office
        /// example: index/hello-world
        /// </param>
        /// <param name="bypassCache">If true - The request will bypass the cache, always going to the server to get the content</param>
        public RawContentResponse GetPublishedContentIncludingMetadata(string url, bool bypassCache)
        {
            return GetPublishedContentIncludingMetadataAsync(url, bypassCache).Result;
        }

        /// <summary>
        /// Requests published content by ID from umbraco asynchronously, including content metadata - With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="id">
        /// The unique ID of the content, which can be found in the umbraco back office
        /// </param>
        public Task<RawContentResponse> GetPublishedContentIncludingMetadataAsync(int id)
        {
            return GetPublishedContentIncludingMetadataAsync(id, false);
        }

        /// <summary>
        /// Requests published content by ID from umbraco asynchronously, including content metadata - With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="id">
        /// The unique ID of the content, which can be found in the umbraco back office
        /// </param>
        /// <param name="bypassCache">If true - The request will bypass the cache, always going to the server to get the content</param>
        public Task<RawContentResponse> GetPublishedContentIncludingMetadataAsync(int id, bool bypassCache)
        {
            return ExecuteContentRequestAsync(id, ApiPath.PublishedContentIdWithTemplate, bypassCache, ContentIdCacheSearch(), AddContentToCache());
        }

        /// <summary>
        /// Requests published content by url from umbraco asynchronously, including content metadata - With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="url">
        /// The unique url of the content generated by umbraco based upon the content tree, which can be found in the umbraco back office
        /// example: index/hello-world
        /// </param>
        public Task<RawContentResponse> GetPublishedContentIncludingMetadataAsync(string url)
        {
            return GetPublishedContentIncludingMetadataAsync(url, false);
        }

        /// <summary>
        /// Requests published content by url from umbraco asynchronously, including content metadata - With the option to bypass the local cache; always going to the server
        /// </summary>
        /// <param name="url">
        /// The unique url of the content generated by umbraco based upon the content tree, which can be found in the umbraco back office
        /// example: index/hello-world
        /// </param>
        /// <param name="bypassCache">If true - The request will bypass the cache, always going to the server to get the content</param>
        public Task<RawContentResponse> GetPublishedContentIncludingMetadataAsync(string url, bool bypassCache)
        {
            string preparedUrl = PrepareUrlParameter(url);
            return ExecuteContentRequestAsync(preparedUrl, ApiPath.PublishedContentByUrlWithTemplate, bypassCache, ContentUrlCacheSearch(), AddContentAndUrlIndexToCache(preparedUrl));            
        }

        /// <summary>
        /// Descends through the umbraco content tree pulling back all descendants of an 'anchor' node.
        /// The result is represented as a flat dictionary; keyed by the content name.
        /// e.g. resultDictionary["Name of content"]
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        public IDictionary<string, RawContentResponse> GetPublishedDescendantsOfFolder(int id)
        {
            return GetPublishedDescendantsOfFolder(id, false);
        }

        /// <summary>
        /// Descends through the umbraco content tree pulling back all descendants of an 'anchor' node.
        /// The result is represented as a flat dictionary; keyed by the content name.
        /// e.g. resultDictionary["Name of content"]
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        /// <param name="bypassCache">If true - The request will bypass the cache, always going to the server to get the content</param>
        public IDictionary<string, RawContentResponse> GetPublishedDescendantsOfFolder(int id, bool bypassCache)
        {
            return GetPublishedDescendantsOfFolderAsync(id, bypassCache).Result;
        }

        public IDictionary<string, RawContentResponse> GetPublishedDescendantsOfFolder(string url)
        {
            return GetPublishedDescendantsOfFolder(url, false);
        }

        public IDictionary<string, RawContentResponse> GetPublishedDescendantsOfFolder(string url, bool bypassCache)
        {
            return GetPublishedDescendantsOfFolderAsync(url, bypassCache).Result;
        }

        /// <summary>
        /// Descends through the umbraco content tree pulling back all descendants of an 'anchor' node.
        /// The result is represented as a flat dictionary; keyed by the content name.
        /// e.g. resultDictionary["Name of content"]
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        public Task<IDictionary<string, RawContentResponse>> GetPublishedDescendantsOfFolderAsync(int id)
        {
            return GetPublishedDescendantsOfFolderAsync(id, false);
        }

        
        /// <summary>
        /// Descends through the umbraco content tree pulling back all descendants of an 'anchor' node.
        /// The result is represented as a flat dictionary; keyed by the content name.
        /// e.g. resultDictionary["Name of content"]
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        /// <param name="bypassCache">If true - The request will bypass the cache, always going to the server to get the content</param>
        public async Task<IDictionary<string, RawContentResponse>> GetPublishedDescendantsOfFolderAsync(int id, bool bypassCache)
        {
            DescendantIdsResponse dependants = await ExecuteDescendantIdsRequestAsync(id, ApiPath.PublishedDescendantIds, bypassCache, DescendantIdCacheSearch(), AddDescendantToCache()).ConfigureAwait(false);
            return await BuildDescendantRawResponse(dependants, bypassCache);
        }

        public Task<IDictionary<string, RawContentResponse>> GetPublishedDescendantsOfFolderAsync(string url)
        {
            return GetPublishedDescendantsOfFolderAsync(url, false);
        }

        public async Task<IDictionary<string, RawContentResponse>> GetPublishedDescendantsOfFolderAsync(string url, bool bypassCache)
        {
            string preparedUrl = PrepareUrlParameter(url);
            DescendantIdsResponse dependants = await ExecuteDescendantIdsRequestAsync(preparedUrl, ApiPath.PublishedDescendantByUrl, bypassCache, DescendantUrlCacheSearch(),
                AddDescendantAndUrlIndexToCache(preparedUrl)).ConfigureAwait(false);
            return await BuildDescendantRawResponse(dependants, bypassCache);
        }

        /// <summary>
        /// Descends through the umbraco content tree pulling back all published contents of a tree picker identified by its property.
        /// The result is represented as a flat dictionary; keyed by the content name.
        /// e.g. resultDictionary["Name of content"]
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        /// <param name="property">The 'property' is the property value for the tree picker and is case-insensitive. It contains the list of Ids from the tree picker</param>
        public IDictionary<string, RawContentResponse> GetPublishedContentOfTreePicker(int id, string property)
        {
            return GetPublishedContentOfTreePicker(id, property, false);
        }

        /// <summary>
        /// Descends through the umbraco content tree pulling back all published contents of a tree picker identified by its property .
        /// The result is represented as a flat dictionary; keyed by the content name.
        /// e.g. resultDictionary["Name of content"]
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        /// <param name="property">The 'property' is the property value for the tree picker and is case-insensitive. It contains the list of Ids from the tree picker</param>
        /// <param name="bypassCache">If true - The request will bypass the cache, always going to the server to get the content</param>
        public IDictionary<string, RawContentResponse> GetPublishedContentOfTreePicker(int id, string property, bool bypassCache)
        {
            return GetPublishedContentOfTreePickerAsync(id,property, bypassCache).Result;
        }

        /// <summary>
        /// Descends through the umbraco content tree pulling back all published contents of a tree picker identified by its property .
        /// The result is represented as a flat dictionary; keyed by the content name.
        /// e.g. resultDictionary["Name of content"]
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        /// <param name="property">The 'property' is the property value for the tree picker and is case-insensitive. It contains the list of Ids from the tree picker</param>
        public Task<IDictionary<string, RawContentResponse>> GetPublishedContentOfTreePickerAsync(int id, string property)
        {
            return GetPublishedContentOfTreePickerAsync(id, property, false);
        }

        /// <summary>
        /// Descends through the umbraco content tree pulling back all published contents of a tree picker identified by its property .
        /// The result is represented as a flat dictionary; keyed by the content name.
        /// e.g. resultDictionary["Name of content"]
        /// </summary>
        /// <param name="id">The unique ID of the content, which can be found in the umbraco back office</param>
        /// <param name="property">The 'property' is the property value for the tree picker and is case-insensitive. It contains the list of Ids from the tree picker</param>
        /// <param name="bypassCache">If true - The request will bypass the cache, always going to the server to get the content</param>
        public async Task<IDictionary<string, RawContentResponse>> GetPublishedContentOfTreePickerAsync(int id, string property, bool bypassCache)
        {
            DescendantIdsResponse content = await ExecuteTreePickerIdsRequestAsync(id, property, ApiPath.PublishedIdsOfTreePicker, bypassCache, 
                TreePickerIdCacheSearch(), AddPickertToCache()).ConfigureAwait(false);
            if (content.DescendantCount == 0)
            {
                return new Dictionary<string, RawContentResponse>();
            }

            var childRequestTasks = content.Descendants.Select(contentId => GetPublishedContentIncludingMetadataAsync(contentId, bypassCache));
            var descendantRawContentResponses = await Task.WhenAll(childRequestTasks).ConfigureAwait(false);

            return descendantRawContentResponses.Where(x => x.Id > 0).ToDictionary(x => x.Name);
        }

        /// <summary>
        /// Exposes the api directly. This method allows you to supply a custom query and response type and do the mappings yourself.
        /// You get error & retry policies; along with caching built in.
        /// The TResponse type needs be fully serialisable.
        /// </summary>
        /// <typeparam name="TResponse">A custom response type to map to : SHOULD BE SERIALIZABLE</typeparam>
        /// <param name="query">An custom query, conforming to a IUmbracoCustomQuery contract</param>
        public Task<TResponse> QueryAsync<TResponse>(IUmbracoCustomQuery<TResponse> query)
        {
            return QueryAsync(query, false);
        }

        /// <summary>
        /// Exposes the api directly. This method allows you to supply a custom query and response type and do the mappings yourself.
        /// You get error & retry policies; along with caching built in.
        /// The TResponse type needs be fully serialisable.
        /// </summary>
        /// <typeparam name="TResponse">A custom response type to map to : SHOULD BE SERIALIZABLE</typeparam>
        /// <param name="query">An custom query, conforming to a IUmbracoCustomQuery contract</param>
        /// <param name="bypassCache"></param>
        public async Task<TResponse> QueryAsync<TResponse>(IUmbracoCustomQuery<TResponse> query, bool bypassCache)
        {
            if (bypassCache == false && cacheIsAvailable)
            {
                TResponse cacheOut;
                if (cache.TryGet(query.UniqueQueryId.ToString(), out cacheOut))
                {
                    return cacheOut;
                }
            }

            var requestWithPolicy = new HttpRequestWithPolicy(this);
            var response = await requestWithPolicy.Execute(() => 
                    httpClient.GetAsync(string.Format(Target.Url.AbsoluteUri + query.RelativeRequestUrl().TrimStart('/')))
                ).ConfigureAwait(false);

            if (response.Result?.IsSuccessStatusCode == false || response.Outcome == OutcomeType.Failure)
            {
                return query.EmptyResponse;
            }

            string jsonResult = await response.Result.Content.ReadAsStringAsync().ConfigureAwait(false);
            TResponse mappedTResponse = query.MapResponse(jsonResult);

            if (cacheIsAvailable && !bypassCache && mappedTResponse != null)
            {
                cache.Add(query.UniqueQueryId, mappedTResponse);

                IEnumerable<CustomQueryCacheValue> cq;
                if (cache.TryGet(CacheSettings.CUSTOM_QUERY_CACHE_KEY, out cq))
                {
                    var indexList = cq.ToList();
                    if (indexList.Any(x => x.CustomQueryKey == query.UniqueQueryId.ToString()) == false)
                    {
                        indexList.Add(new CustomQueryCacheValue
                        {
                            CustomQueryKey = query.UniqueQueryId.ToString(),
                            ContentId = query.AssociatedContentId.ToString()
                        });
                        cache.Add(CacheSettings.CUSTOM_QUERY_CACHE_KEY, indexList);
                    }
                }
                else
                {
                    var newCacheIndex = new List<CustomQueryCacheValue>
                    {
                        new CustomQueryCacheValue { CustomQueryKey = query.UniqueQueryId.ToString(), ContentId = query.AssociatedContentId.ToString() }
                    };
                    cache.Add(CacheSettings.CUSTOM_QUERY_CACHE_KEY, newCacheIndex);
                }
            }

            return mappedTResponse;
        }


        /// <summary>
        /// Gives the ability to customise the current umbraco request with options.
        /// All request methods will use these settings.
        /// </summary>
        /// <returns>Returns the request object (this)</returns>
        public UmbracoClientRequest WithSettings(UmbracoRequestSettings requestSettings)
        {
            if (requestSettings == null)
                throw new ArgumentNullException(nameof(requestSettings));

            settings = requestSettings;

            return this;
        }

        private async Task<RawContentResponse> ExecuteContentRequestAsync<TId>(TId contentIdentifier, string endpoint, bool bypassCache, TryFunc<string, RawContentResponse> tryGetFromCache, Action<string, RawContentResponse> addToCache)
        {
            if (bypassCache == false && cacheIsAvailable)
            {
                RawContentResponse cacheOut;
                if (tryGetFromCache(contentIdentifier.ToString(), out cacheOut))
                {
                    return cacheOut;
                }
            }
           
            var requestWithPolicy = new HttpRequestWithPolicy(this);
            var response = await requestWithPolicy.Execute(() =>
                httpClient.GetAsync(string.Format((Target.Url.AbsoluteUri + endpoint), contentIdentifier))
                    ).ConfigureAwait(false);


            if (response.Result?.IsSuccessStatusCode == false || response.Outcome == OutcomeType.Failure)
            {
                // We don't want to throw exceptions if we can't return any content. 
                // Let the caller deal with the empty result if they deem it critical.
                return RawContentResponse.Empty;
            }

            string jsonResult = await response.Result.Content.ReadAsStringAsync().ConfigureAwait(false);
            RawContentResponse rawContentResponse = RawContentResponseFactory.Build(jsonResult);

            if (cacheIsAvailable && !bypassCache && !string.IsNullOrWhiteSpace(rawContentResponse.RenderedContent))
            {
                addToCache(rawContentResponse.Id.ToString(), rawContentResponse);
            }

            return rawContentResponse;
        }

        private async Task<DescendantIdsResponse> ExecuteDescendantIdsRequestAsync<TId>(TId contentIdentifier, string endpoint, bool bypassCache, TryFunc<string, DescendantIdsResponse> tryGetFromCache, Action<string, DescendantIdsResponse> addToCache)
        {
            if (bypassCache == false && cacheIsAvailable)
            {
                DescendantIdsResponse cacheOut;
                if (tryGetFromCache(contentIdentifier.ToString(), out cacheOut))
                {
                    return cacheOut;
                }
            }

            var requestWithPolicy = new HttpRequestWithPolicy(this);
            var response = await requestWithPolicy.Execute(() =>
                    httpClient.GetAsync(string.Format((Target.Url.AbsoluteUri + endpoint), contentIdentifier))
                ).ConfigureAwait(false);

            if (response.Result?.IsSuccessStatusCode == false || response.Outcome == OutcomeType.Failure)
            {
                return DescendantIdsResponse.Empty;
            }

            string jsonResult = await response.Result.Content.ReadAsStringAsync().ConfigureAwait(false);
            DescendantIdsResponse descendantIdsResponse = JsonConvert.DeserializeObject<DescendantIdsResponse>(jsonResult);

            if (cacheIsAvailable && !bypassCache && (descendantIdsResponse.DescendantCount > 0))
            {
                addToCache(descendantIdsResponse.Origin.ToString(), descendantIdsResponse);
            }

            return descendantIdsResponse;
        }

        private async Task<DescendantIdsResponse> ExecuteTreePickerIdsRequestAsync(int id,string property, string endpoint, bool bypassCache, TryFunc<string, DescendantIdsResponse> tryGetFromCache, Action<string, DescendantIdsResponse> addToCache)
        {
            if (bypassCache == false && cacheIsAvailable)
            {
                DescendantIdsResponse cacheOut;
                if (tryGetFromCache(id.ToString(), out cacheOut))
                {
                    return cacheOut;
                }
            }

            var requestWithPolicy = new HttpRequestWithPolicy(this);
            var response = await requestWithPolicy.Execute(() =>
                    httpClient.GetAsync(string.Format((Target.Url.AbsoluteUri + endpoint), id, property))
                ).ConfigureAwait(false);

            if (response.Result?.IsSuccessStatusCode == false || response.Outcome == OutcomeType.Failure)
            {
                return DescendantIdsResponse.Empty;
            }

            string jsonResult = await response.Result.Content.ReadAsStringAsync().ConfigureAwait(false);
            DescendantIdsResponse treePickerIdsResponse = JsonConvert.DeserializeObject<DescendantIdsResponse>(jsonResult);

            if (cacheIsAvailable && !bypassCache && (treePickerIdsResponse.DescendantCount > 0))
            {
                addToCache(treePickerIdsResponse.Origin.ToString(), treePickerIdsResponse);
            }

            return treePickerIdsResponse;
        }

        private async Task<IDictionary<string, RawContentResponse>> BuildDescendantRawResponse(DescendantIdsResponse descendant, bool bypassCache)
        {
            if (descendant.DescendantCount == 0)
            {
                return new Dictionary<string, RawContentResponse>();
            }

            var childRequestTasks = descendant.Descendants.Select(depId => GetPublishedContentIncludingMetadataAsync(depId, bypassCache));
            var descendantRawContentResponses = await Task.WhenAll(childRequestTasks).ConfigureAwait(false);

            return descendantRawContentResponses.Where(x => x.Id > 0).ToDictionary(x => x.Name);
        }

        private Action<string, RawContentResponse> AddContentAndUrlIndexToCache(string url)
        {
            return (id, content) =>
            {
                cache.Add(CacheSettings.GetContentKey(id), content);
                cache.Add(url, id);
            };
        }

        private Action<string, RawContentResponse> AddContentToCache()
        {
            return (id, content) =>
            {
                cache.Add(CacheSettings.GetContentKey(id), content);
            };
        }

        private Action<string, DescendantIdsResponse> AddDescendantToCache()
        {
            return (id, content) =>
            {
                cache.Add(CacheSettings.GetDescendantsKey(id), content);
            };
        }

        private Action<string, DescendantIdsResponse> AddPickertToCache()
        {
            return (id, content) =>
            {
                cache.Add(CacheSettings.GetPickerKey(id), content);
            };
        }

        private Action<string, DescendantIdsResponse> AddDescendantAndUrlIndexToCache(string url)
        {
            return (id, content) =>
            {
                cache.Add(CacheSettings.GetDescendantsKey(id), content);
                cache.Add(url, id);
            };
        }

        private TryFunc<string, RawContentResponse> ContentUrlCacheSearch()
        {
            return (string url, out RawContentResponse value) =>
            {
                value = null;
                string contentId;
                if (!cache.TryGet(url, out contentId))
                    return false;

                RawContentResponse content;
                if (!cache.TryGet(CacheSettings.GetContentKey(contentId), out content))
                    return false;

                value = content;
                return true;
            };
        }

        private TryFunc<string, RawContentResponse> ContentIdCacheSearch()
        {
            return (string id, out RawContentResponse value) =>
            {
                value = null;
                RawContentResponse content;
                if (!cache.TryGet(CacheSettings.GetContentKey(id), out content))
                    return false;

                value = content;
                return true;
            };
        }

        private TryFunc<string, DescendantIdsResponse> DescendantUrlCacheSearch()
        {
            return (string url, out DescendantIdsResponse value) =>
            {
                value = null;
                string contentId;
                if (!cache.TryGet(url, out contentId))
                    return false;

                DescendantIdsResponse content;
                if (!cache.TryGet(CacheSettings.GetDescendantsKey(contentId), out content))
                    return false;

                value = content;
                return true;
            };
        }

        private TryFunc<string, DescendantIdsResponse> DescendantIdCacheSearch()
        {
            return (string id, out DescendantIdsResponse value) =>
            {
                value = null;
                DescendantIdsResponse content;
                if (!cache.TryGet(CacheSettings.GetDescendantsKey(id), out content))
                    return false;

                value = content;
                return true;
            };
        }

        private TryFunc<string, DescendantIdsResponse> TreePickerIdCacheSearch()
        {
            return (string id, out DescendantIdsResponse value) =>
            {
                value = null;
                DescendantIdsResponse content;
                if (!cache.TryGet(CacheSettings.GetPickerKey(id), out content))
                    return false;

                value = content;
                return true;
            };
        }

        private string PrepareUrlParameter(string url)
        {
            return HttpUtility.UrlEncode(url.Trim('/'));
        }

        private delegate bool TryFunc<in TIn, TOut>(TIn input, out TOut output);


        private class HttpRequestWithPolicy
        {
            private readonly UmbracoRequestSettings settings;
            private readonly UmbracoClientRequest outer;

            public HttpRequestWithPolicy(UmbracoClientRequest outer)
            {
                this.outer = outer;
                this.settings = outer.settings;
            }

            public Task<PolicyResult<HttpResponseMessage>> Execute(Func<Task<HttpResponseMessage>> fn)
            {
                return Policy
                    .HandleResult<HttpResponseMessage>(r => settings.HttpStatusCodesWorthRetrying.Contains((int)r.StatusCode))
                    .WaitAndRetryAsync(
                        settings.NumberOfRetries,
                        i =>
                        {
                            int modifier = settings.RetryIntervalModifier * i;
                            return TimeSpan.FromTicks(settings.RetryInterval.Ticks * modifier);
                        },
                        onRetry: (res, timespan) => outer.OnRequestRetry?.Invoke(this, new OnRequestFailureEventArgs { ResponseMessage = res.Result, RequestException = res.Exception }))
                    .ExecuteAndCaptureAsync(fn);
            }
        }
    }

    /// <summary>
    /// A delegate which describes the signature of a request retry handler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void OnRequestFailureHandler(object sender, OnRequestFailureEventArgs args);
}
