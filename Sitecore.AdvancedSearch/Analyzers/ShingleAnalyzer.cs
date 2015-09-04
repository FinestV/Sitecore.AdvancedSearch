using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Shingle;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Sitecore.AdvancedSearch.Analyzers
{
    public class ShingleAnalyzer : Analyzer
    {
        private readonly Version _version;

        public ShingleAnalyzer(Version version)
        {
            _version = version;
        }
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            //Need to account for the | breaks in relatedcontent
            var tokenizedInput = new LowerCaseFilter(new StandardFilter(new StandardTokenizer(_version, reader)));
            //return new ShingleFilter(tokenizedInput, 4);

            var output = new ShingleFilter(tokenizedInput, 4);
            //output.SetOutputUnigrams(false);
            return output;
        }
    }
}
