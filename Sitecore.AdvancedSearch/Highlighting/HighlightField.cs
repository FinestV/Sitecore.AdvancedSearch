namespace Sitecore.AdvancedSearch.Highlighting
{
    public class HighlightField
    {
        public string FieldName { get; set; }
        public bool RemoveShortContentChunks { get; set; }
        public HighlightField(string fieldName, bool removeShortContentChunks = false)
        {
            FieldName = fieldName;
            RemoveShortContentChunks = removeShortContentChunks;
        }
    }
}
