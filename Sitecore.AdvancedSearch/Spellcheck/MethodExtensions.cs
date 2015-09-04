using Lucene.Net.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.AdvancedSearch.Spellcheck
{
    public static class MethodExtensions
    {
        public static string SpellCheck(this SpellChecker.Net.Search.Spell.SpellChecker spellChecker, string terms)
        {
            var splitTerms = terms.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var checkedTerms = spellChecker.SpellCheck(splitTerms);
            return string.Join(" ", checkedTerms);
        }

        public static List<string> SpellCheck(this SpellChecker.Net.Search.Spell.SpellChecker spellChecker, List<string> terms)
        {
            var checkedTerms = new List<string>();
            foreach (var term in terms)
            {
                if (!StopAnalyzer.ENGLISH_STOP_WORDS_SET.Contains(term) && !spellChecker.Exist(term))
                {
                    checkedTerms.Add(spellChecker.SuggestSimilar(term, 7, null, null, true).FirstOrDefault() ?? term);
                }
                else
                {
                    checkedTerms.Add(term);
                }
            }
            return checkedTerms;
        }
    }
}
