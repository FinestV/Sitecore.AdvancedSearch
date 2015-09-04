using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Analysis.Standard;
using SF.Snowball.Ext;
using Lucene.Net.Util;

namespace Sitecore.AdvancedSearch.Analyzers
{
    public class SnowballAnalyzer : Analyzer
    {
        private readonly Version _version;
        public SnowballAnalyzer(Version version)
        {
            _version = version;
        }
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result = new SnowballFilter(
                new StopFilter(true,
                    new LowerCaseFilter(
                        new StandardFilter(
                            new StandardTokenizer(_version, reader))), StopAnalyzer.ENGLISH_STOP_WORDS_SET),
                new EnglishStemmer());
            return result;
        }
    }
}
