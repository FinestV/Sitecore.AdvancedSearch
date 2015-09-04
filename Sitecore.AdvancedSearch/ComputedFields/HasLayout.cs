using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.AdvancedSearch.ComputedFields
{
    public class HasLayout : IComputedIndexField
    {
        public string FieldName { get; set; }
        public string ReturnType { get; set; }

        public object ComputeFieldValue(IIndexable indexable)
        {
            var item = (Item)(indexable as SitecoreIndexableItem);
            Assert.ArgumentNotNull(item, "item");

            if (item.Visualization != null && item.Visualization.Layout != null && !item.Paths.LongID.Contains(ItemIDs.TemplateRoot.ToString()))
            {
                return true;
            }
            return null;
        }
    }
}
