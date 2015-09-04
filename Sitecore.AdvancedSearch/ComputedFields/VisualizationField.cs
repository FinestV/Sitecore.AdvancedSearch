using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;

namespace Sitecore.AdvancedSearch.ComputedFields
{
    /// <summary>
    /// Crawls the renderings on an item and includes their content in the index
    /// </summary>
    public class VisualizationField : IComputedIndexField
    {
        private readonly HashSet<string> _textFieldTypes = new HashSet<string>(new[]
        {
            "Single-Line Text", 
            "Rich Text", 
            "Multi-Line Text", 
            "text", 
            "rich text", 
            "html", 
            "memo", 
            "Word Document"
        });

        public string FieldName { get; set; }
        public string ReturnType { get; set; }

        public object ComputeFieldValue(IIndexable indexable)
        {
            var item = (Item)(indexable as SitecoreIndexableItem);
            Assert.ArgumentNotNull(item, "item");

            if (!ShouldIndexItem(item))
            {
                return null;
            }
            List<Item> dataSources = Globals.LinkDatabase.GetReferences(item)
                                           .Where(link => ShouldProcessLink(link, item))
                                           .Select(link => link.GetTargetItem())
                                           .Where(targetItem => targetItem != null)
                                           .Distinct().ToList();
            dataSources.Add(item);

            var result = new StringBuilder();
            foreach (var dataSource in dataSources.Where(ShouldIndexDataSource))
            {
                dataSource.Fields.ReadAll();
                foreach (var field in dataSource.Fields.Where(ShouldIndexField))
                {
                    result.AppendLine(IndexOperationsHelper.StripHtml(field.Value));;
                }
            }
            return result.ToString();
        }

        protected virtual bool ShouldIndexItem(Item item)
        {
            //only items w/ layout that are not template standard values
            return item.Visualization != null && item.Visualization.Layout != null && !item.Paths.LongID.Contains(ItemIDs.TemplateRoot.ToString());
        }

        protected virtual bool ShouldProcessLink(ItemLink link, Item sourceItem)
        {
            //layout field references in the same database
            return link.SourceFieldID == FieldIDs.LayoutField && link.SourceDatabaseName == sourceItem.Database.Name;
        }

        protected virtual bool ShouldIndexDataSource(Item item)
        {
            //don't process references to renderings
            return !item.Paths.LongID.Contains(ItemIDs.LayoutRoot.ToString());
        }

        protected virtual bool ShouldIndexField(Field field)
        {
            //process non-empty text fields that are not part of the standard template
            return !field.Name.StartsWith("__") && IsTextField(field) && !string.IsNullOrEmpty(field.Value);
        }

        protected virtual bool IsTextField(Field field)
        {
            return _textFieldTypes.Contains(field.Type);
        }
    }
}
