namespace Umbraco.Caching
{
    public class TombolaCacheSettings
    {
        public string CacheProviderTypeName { get; set; }

        public const string CUSTOM_QUERY_CACHE_KEY = "CustomQueriesIndex";
        private const string CONTENT_KEY = "{0}";
        private const string DECENDANTS_KEY = "{0}_descendants";
        private const string PICKER_KEY = "{0}_picker";
        

        public static string GetContentKey(string id)
        {
            return string.Format(CONTENT_KEY, id);
        }

        public static string GetDescendantsKey(string id)
        {
            return string.Format(DECENDANTS_KEY, id);
        }

        public static string GetPickerKey(string id)
        {
            return string.Format(PICKER_KEY, id);
        }

     
    }
}
