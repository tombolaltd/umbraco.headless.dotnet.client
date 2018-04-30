using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Umbraco.Client
{
    public static class RawContentResponseFactory
    {

        public static RawContentResponse Build(string jsonResults)
        {
            var building = JsonConvert.DeserializeObject<RawContentResponse>(jsonResults);
            GenerateMetas(building);
            return building;
        }


        private static void GenerateMetas(RawContentResponse building)
        {
            if (building.PropertyCollection != null)
            {
                var pagetitle = building.PropertyCollection.AsQueryable().Where(kvp => kvp.Key.Equals("sEOTitle", StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                var pageDescription = building.PropertyCollection.AsQueryable().Where(kvp => kvp.Key.Equals("sEODescription", StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                var keywords = building.PropertyCollection.AsQueryable().Where(kvp => kvp.Key.Equals("sEOKeywords", StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                var robots = building.PropertyCollection.AsQueryable().Where(kvp => kvp.Key.Equals("sEORobots", StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                var metatags = building.PropertyCollection.AsQueryable().Where(kvp => kvp.Key.Equals("sEOMetaTags", StringComparison.OrdinalIgnoreCase)).SingleOrDefault();

                var tagHtmlRepresentation = new StringBuilder();
                var metas = new List<KeyValuePair<string, string>>();

                ConfigureTitleTag(building.Name, pagetitle, tagHtmlRepresentation, metas);
                ConfigureDescriptionTag(pageDescription, tagHtmlRepresentation, metas);
                ConfigureKeywordsTag(keywords, tagHtmlRepresentation, metas);
                ConfigureRobotsTag(robots, tagHtmlRepresentation, metas);
                ConfigureMetaTags(metatags, tagHtmlRepresentation, metas);

                building.MetaTagCollection = metas;
                building.RenderedMetaTags = tagHtmlRepresentation.ToString();
            }
        }

        private static void ConfigureTitleTag(string pageName, KeyValuePair<string, string> pagetitle, StringBuilder tagHtmlRepresentation, List<KeyValuePair<string, string>> metas)
        {
            string metaTitle = string.IsNullOrEmpty(pagetitle.Value) ? pageName : pagetitle.Value;

            tagHtmlRepresentation.Append($"<meta property=\"og:title\" content=\"{metaTitle}\">");
            tagHtmlRepresentation.Append($"<meta name=\"twitter:title\" content=\"{metaTitle}\">");
            metas.Add(new KeyValuePair<string, string>("title", metaTitle));
            metas.Add(new KeyValuePair<string, string>("og:title", metaTitle));
            metas.Add(new KeyValuePair<string, string>("twitter:title", metaTitle));
        }

        private static void ConfigureDescriptionTag(KeyValuePair<string, string> pageDescription, StringBuilder tagHtmlRepresentation, List<KeyValuePair<string, string>> metas)
        {
            if (!string.IsNullOrEmpty(pageDescription.Value))
            {
                tagHtmlRepresentation.Append($"<meta name=\"description\" content=\"{pageDescription.Value}\">");
                tagHtmlRepresentation.Append($"<meta property=\"og:description\" content=\"{pageDescription.Value}\">");
                tagHtmlRepresentation.Append($"<meta name=\"twitter:description\" content=\"{pageDescription.Value}\">");
                metas.Add(new KeyValuePair<string, string>("description", pageDescription.Value));
                metas.Add(new KeyValuePair<string, string>("og:description", pageDescription.Value));
                metas.Add(new KeyValuePair<string, string>("twitter:description", pageDescription.Value));
            }
        }

        private static void ConfigureKeywordsTag(KeyValuePair<string, string> keywords, StringBuilder tagHtmlRepresentation, List<KeyValuePair<string, string>> metas)
        {
            if (!string.IsNullOrEmpty(keywords.Value))
            {
                tagHtmlRepresentation.Append($"<meta name=\"keywords\" content=\"{keywords.Value}\">");
                metas.Add(new KeyValuePair<string, string>("keywords", keywords.Value));
            }
        }

        private static void ConfigureRobotsTag(KeyValuePair<string, string> robots, StringBuilder tagHtmlRepresentation, List<KeyValuePair<string, string>> metas)
        {
            if (!string.IsNullOrEmpty(robots.Value))
            {
                tagHtmlRepresentation.Append($"<meta name=\"robots\" content=\"{robots.Value}\">");
                metas.Add(new KeyValuePair<string, string>("robots", robots.Value));
            }
        }

        private static void ConfigureMetaTags(KeyValuePair<string, string> metatags, StringBuilder tagHtmlRepresentation, List<KeyValuePair<string, string>> metas)
        {
            if (!string.IsNullOrEmpty(metatags.Value))
            {
                var metaTagItems = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(metatags.Value);

                if ((metaTagItems != null) && (metaTagItems.Count > 0))
                {
                    foreach (var item in metaTagItems)
                    {
                        metas.Add(new KeyValuePair<string, string>(item.Key, item.Value));
                        string metaType = item.Key.StartsWith("og:") ? "property" : "name";
                        tagHtmlRepresentation.Append($"<meta {metaType}=\"{item.Key}\" content=\"{item.Value}\">");
                    }
                }
            }
        }
    }
}