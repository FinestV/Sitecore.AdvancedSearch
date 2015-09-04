using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Sitecore.AdvancedSearch.Analyzers
{
    public class StemmingAnalyzerNoStopFilter : Analyzer
    {
        private readonly Version _version;
        public StemmingAnalyzerNoStopFilter(Version version)
        {
            _version = version;
        }
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result =
                new LowerCaseFilter(

                    new StandardTokenizer(_version, reader));
            return result;
        }
    }
}
