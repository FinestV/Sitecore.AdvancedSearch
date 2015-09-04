using System;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Analysis.Standard;
using SF.Snowball.Ext;
using Version = Lucene.Net.Util.Version;

namespace Sitecore.AdvancedSearch.Analyzers
{
    public class EdgeNGramWithStemmingAnalyzer : Analyzer
    {
        private Version _version;
        private int _mingram;
        private int _maxgram;
        public EdgeNGramWithStemmingAnalyzer(Version version, string mingram, string maxgram)
        {
            _version = version;
            _mingram = System.Convert.ToInt16(mingram);
            _maxgram = System.Convert.ToInt16(maxgram);
        }
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            //Apply standard tokenizer to input
            var tokenizedInput = new StandardTokenizer(_version, reader);

            //Apply standard, lowercase and English stop words filters to input
            var filteredInput = new SnowballFilter(new StopFilter(true, new LowerCaseFilter(new StandardFilter(tokenizedInput)),
                StopAnalyzer.ENGLISH_STOP_WORDS_SET), new EnglishStemmer());

            //Apply EdgeNGram filter to front of words
            //Min size of grams max size of grams
            var grammedInput = new EdgeNGramTokenFilter(filteredInput, Side.FRONT, _mingram, _maxgram);

            return grammedInput;
        }
    }
}
