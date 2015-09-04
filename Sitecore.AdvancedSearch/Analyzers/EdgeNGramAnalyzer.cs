using System;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Standard;
using Version = Lucene.Net.Util.Version;

namespace Sitecore.AdvancedSearch.Analyzers
{
    /// <summary>
    /// NGramAnalyzer search analyzer. Tokenizes content into shingles to support TypeAhead functionality.
    /// </summary>
    public class EdgeNGramAnalyzer : Analyzer
    {
        private readonly Version _version;
        private readonly int _mingram;
        private readonly int _maxgram;
        public EdgeNGramAnalyzer(Version version, string mingram, string maxgram)
        {
            _version = version;
            _mingram = System.Convert.ToInt16(mingram);
            _maxgram = System.Convert.ToInt16(maxgram);
        }
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            //Apply standard tokenizer to input
            var tokenizedInput = new StandardTokenizer(_version, reader);

            //TODO: do we want to remove stop words from auto complete?
            //Apply standard, lowercase and English stop words filters to input
            var filteredInput = new StopFilter(true, new LowerCaseFilter(new StandardFilter(tokenizedInput)),
                StopAnalyzer.ENGLISH_STOP_WORDS_SET);

            //Apply EdgeNGram filter to front of words
            //Min size of grams max size of grams
            var grammedInput = new EdgeNGramTokenFilter(filteredInput, Side.FRONT, _mingram, _maxgram);

            return grammedInput;
        }
    }
}
