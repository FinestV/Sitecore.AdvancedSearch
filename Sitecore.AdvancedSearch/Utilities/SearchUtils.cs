using Sitecore.AdvancedSearch.Models;
using Sitecore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;

namespace Sitecore.AdvancedSearch.Utilities
{
    public static class SearchUtils
    {
        public static readonly string RelatedContentField = Settings.GetSetting("RelatedContentField");
        public static readonly string MediaContentField = Settings.GetSetting("MediaContentField");
        public static readonly string UrlLinkField = Settings.GetSetting("UrlLinkField");
        /// <summary>
        /// Returns a search predicate using the related content fields set up in the provided Search configuration
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="queryTerms">Terms of query</param>
        /// <returns>A search predicate for use with a Sitecore Linq Queryable</returns>
        public static Expression<Func<T, bool>> GetRelatedContentPredicate<T>(IEnumerable<string> queryTerms) where T : SearchResultItem
        {
            var predicate = PredicateBuilder.False<T>();//.Or(x => x.RelatedContent == "").Boost(1.5f);
            foreach (var queryTerm in queryTerms)
            {
                var term = queryTerm;
                //predicate = predicate.Or(x => x[RelatedContentField] == term)
                //                     .Or(x => x[MediaContentField] == term);
                predicate = predicate.Or(x => x.RelatedContent == term || x.MediaContent == term);
                //.Or(x => x.MediaContent == term);
            }


            return predicate;
        }

        public static Expression<Func<T, bool>> GetRelatedContentPredicate<T>(string query) where T : SearchResultItem
        {
            //Exact Phrase match. Apply boost if phrase match.
            var predicate = PredicateBuilder.Create<T>(x => x.RelatedContent == query || x.MediaContent == query).Boost(1.9f);
            foreach (var queryTerm in query.Split(' '))
            {
                var term = queryTerm;
                predicate = predicate.Or(x => x.RelatedContent == term || x.MediaContent == term);
            }
            return predicate;
        }

        public static string FlattenFragments(this string[] fragments, string suffix = "...")
        {
            return string.Join(" ", fragments.Select(f => f + suffix));
        }
    }
}
