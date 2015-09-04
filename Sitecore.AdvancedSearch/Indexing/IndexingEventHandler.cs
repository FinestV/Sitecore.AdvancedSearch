using System;
using System.Linq;
using Lucene.Net.Index;
using Sitecore.AdvancedSearch.Spellcheck;
using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Abstractions;
using Sitecore.ContentSearch.LuceneProvider.Sharding;
using Sitecore.Diagnostics;
using SpellChecker.Net.Search.Spell;

namespace Sitecore.AdvancedSearch.Indexing
{
    public class IndexingEventHandler
    {
        private static readonly string[] SpellcheckIndexes = Settings.GetSetting("SpellCheckerIndexes").Split('|');
        private static readonly string SpellCheckerSourceField = Settings.GetSetting("SpellCheckerSourceField");

        public void UpdateSpellCheckerDictionary(object sender, EventArgs args)
        {
            Assert.IsNotNull(args, "event args");
            string indexName = ContentSearchManager.Locator.GetInstance<IEvent>().ExtractParameter<string>(args, 0);
            Assert.IsNotNullOrEmpty(indexName, "index name");
            bool fullRebuild = (bool)ContentSearchManager.Locator.GetInstance<IEvent>().ExtractParameter(args, 1);
            RunAsync(() => UpdateSpellCheckerDictionaryAsync(indexName, fullRebuild), indexName);
        }

        public void UpdateSpellCheckerDictionaryAsync(string indexName, bool fullRebuild)
        {
            if (ContentSearchManager.SearchConfiguration.Indexes.ContainsKey(indexName))
            {
                ISearchIndex index = ContentSearchManager.GetIndex(indexName);
                if (index != null && SpellcheckIndexes.Contains(index.Name))
                {
                    using (var spellchecker = SpellCheckerManager.GetSpellChecker(index.Name))
                    {
                        //Clear the spellcheck dictionary if it is a full rebuild
                        if (fullRebuild)
                        {
                            spellchecker.ClearIndex();
                        }
                        //Add to the spellcheck dictionary for each shard in the index
                        foreach (var shard in index.Shards.Select(s => s as LuceneShard).Where(s => s != null))
                        {
                            using (var reader = IndexReader.Open(shard.Directory, true))
                            {
                                spellchecker.IndexDictionary(new LuceneDictionary(reader, SpellCheckerSourceField));
                            }
                        }
                    }
                }
            }
        }
        public static void RunAsync(Action action, string indexName)
        {
            try
            {
                action.BeginInvoke(null, null);
            }
            catch (Exception ex)
            {
                Log.Error("Error occurred while updating spellchecker dictionary for index " + indexName, ex, typeof(IndexingEventHandler));
            }
        }
    }
}
