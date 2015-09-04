using Sitecore.ContentSearch;

namespace Sitecore.AdvancedSearch.Models
{
    public class SearchSuggestion
    {
        [IndexField("word")]
        public virtual string Suggestion { get; set; }
    }
}
