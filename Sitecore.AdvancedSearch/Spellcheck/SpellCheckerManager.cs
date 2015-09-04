using Sitecore.Configuration;
using System.IO;

namespace Sitecore.AdvancedSearch.Spellcheck
{
    public static class SpellCheckerManager
    {
        private static readonly string SpellCheckerPrefix = Settings.GetSetting("SpellCheckerPrefix");
        public static SpellChecker.Net.Search.Spell.SpellChecker GetSpellChecker(string indexName)
        {
            var path = Path.Combine(Settings.IndexFolder, SpellCheckerPrefix + indexName);
            return new SpellChecker.Net.Search.Spell.SpellChecker(Lucene.Net.Store.FSDirectory.Open(path));
        }

    }
}
