using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Sitecore.AdvancedSearch.Analyzers
{
    public class LowerCaseAnalyzer : Analyzer
    {
        private Version _version;

        public LowerCaseAnalyzer(Version version)
        {
            _version = version;
        }
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var tokenizedInput = new StandardTokenizer(_version, reader);
            return new LowerCaseFilter(tokenizedInput);
        }
    }
}
