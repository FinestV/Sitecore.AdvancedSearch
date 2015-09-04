using Sitecore.ContentSearch;

namespace Sitecore.AdvancedSearch.Models
{
    public class SearchResultItem : Sitecore.ContentSearch.SearchTypes.SearchResultItem
    {
        [IndexField("title")]
        public virtual string Title { get; set; }

        [IndexField("relatedcontent")]
        public virtual string RelatedContent { get; set; }

        [IndexField("mediacontent")]
        public virtual string MediaContent { get; set; }
    }
}
