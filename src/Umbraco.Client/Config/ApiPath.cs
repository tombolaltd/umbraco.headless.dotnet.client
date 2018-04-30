namespace Umbraco.Client.Config
{
    static class ApiPath
    {
        public const string CacheSettings = "cache/settings/";
        public const string Content = "content/";
        public const string PublishedContent = Content + "published/";

        // content by ID
        public const string ContentId = Content + "{0}/";
        public const string ContentIdWithTemplate = Content + "{0}/template/";
        public const string PublishedContentId = PublishedContent + "{0}/";
        public const string PublishedContentIdWithTemplate = PublishedContent + "{0}/template/";
        public const string PublishedContentIdSpecifiedTemplate = PublishedContent + "{0}/specifiedtemplate";

        // content by Name
        public const string PublishedContentByUrl = PublishedContent + "byurl?url={0}";
        public const string PublishedContentByUrlWithTemplate = PublishedContent + "byurl/template?url={0}";

        // descendant IDs
        public const string PublishedDescendantIds = PublishedContentId + "descendantids";
        public const string PublishedDescendantByUrl = PublishedContent + "byurl/descendantids?url={0}";

        // Tree picker IDs
        public const string PublishedIdsOfTreePicker = PublishedContentId + "treePickerIds?property={1}";
    }
}