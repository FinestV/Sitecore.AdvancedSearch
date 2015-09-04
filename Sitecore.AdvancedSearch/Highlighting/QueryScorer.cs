#region

using System;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Index;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Support;
using Lucene.Net.Util;
using Lucene.Net.Search;

#endregion

namespace Sitecore.AdvancedSearch.Highlighting
{

    /// <summary>
    /// Custom query scorer for the highlighter.
    /// </summary>
    public class QueryScorer : IScorer
    {
        #region Fields

        private float totalScore;
        private ISet<string> foundTerms;
        private IDictionary<string, WeightedSpanTerm> fieldWeightedSpanTerms;
        private float maxTermWeight;
        private int position = -1;
        private String defaultField;
        private ITermAttribute termAtt;
        private IPositionIncrementAttribute posIncAtt;
        private bool expandMultiTermQuery = true;
        private Query query;
        private String field;
        private IndexReader reader;
        private bool skipInitExtractor;
        private bool wrapToCaching = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryScorer"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        public QueryScorer(Query query)
        {
            Init(query, null, null, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryScorer"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="field">The field.</param>
        public QueryScorer(Query query, String field)
        {
            Init(query, field, null, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryScorer"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="reader">The reader.</param>
        /// <param name="field">The field.</param>
        public QueryScorer(Query query, IndexReader reader, String field)
        {
            Init(query, field, reader, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryScorer"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="reader">The reader.</param>
        /// <param name="field">The field.</param>
        /// <param name="defaultField">The default field.</param>
        public QueryScorer(Query query, IndexReader reader, String field, String defaultField)
        {
            this.defaultField = StringHelper.Intern(defaultField);
            Init(query, field, reader, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryScorer"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="field">The field.</param>
        /// <param name="defaultField">The default field.</param>
        public QueryScorer(Query query, String field, String defaultField)
        {
            this.defaultField = StringHelper.Intern(defaultField);
            Init(query, field, null, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryScorer"/> class.
        /// </summary>
        /// <param name="weightedTerms">The weighted terms.</param>
        public QueryScorer(WeightedSpanTerm[] weightedTerms)
        {
            this.fieldWeightedSpanTerms = new HashMap<String, WeightedSpanTerm>(weightedTerms.Length);

            foreach (WeightedSpanTerm t in weightedTerms)
            {
                WeightedSpanTerm existingTerm = fieldWeightedSpanTerms[t.Term];

                if ((existingTerm == null) ||
                    (existingTerm.Weight < t.Weight))
                {
                    // if a term is defined more than once, always use the highest
                    // scoring Weight
                    fieldWeightedSpanTerms[t.Term] = t;
                    maxTermWeight = Math.Max(maxTermWeight, t.Weight);
                }
            }
            skipInitExtractor = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the fragment score.
        /// </summary>
        /// <value>
        /// The fragment score.
        /// </value>
        public float FragmentScore
        {
            get { return totalScore; }
        }

        /// <summary>
        /// Gets the maximum term weight.
        /// </summary>
        /// <value>
        /// The maximum term weight.
        /// </value>
        public float MaxTermWeight
        {
            get { return maxTermWeight; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the token score.
        /// </summary>
        /// <returns></returns>
        public float GetTokenScore()
        {
            position += posIncAtt.PositionIncrement;
            String termText = termAtt.Term;

            WeightedSpanTerm weightedSpanTerm;

            if ((weightedSpanTerm = fieldWeightedSpanTerms[termText]) == null)
            {
                return 0;
            }

            if (weightedSpanTerm.IsPositionSensitive() &&
                !weightedSpanTerm.CheckPosition(position))
            {
                return 0;
            }

            float score = weightedSpanTerm.Weight;

            // found a query term - is it unique in this doc?
            if (!foundTerms.Contains(termText))
            {
                totalScore += score;
                foundTerms.Add(termText);
            }

            return score;
        }

        /// <summary>
        /// Initializes the specified token stream.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns></returns>
        public TokenStream Init(TokenStream tokenStream)
        {
            position = -1;
            termAtt = tokenStream.AddAttribute<ITermAttribute>();
            posIncAtt = tokenStream.AddAttribute<IPositionIncrementAttribute>();
            if (!skipInitExtractor)
            {
                if (fieldWeightedSpanTerms != null)
                {
                    fieldWeightedSpanTerms.Clear();
                }
                return InitExtractor(tokenStream);
            }
            return null;
        }

        /// <summary>
        /// Gets the weighted span term.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public WeightedSpanTerm GetWeightedSpanTerm(String token)
        {
            return fieldWeightedSpanTerms[token];
        }

        /// <summary>
        /// Initializes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="field">The field.</param>
        /// <param name="reader">The reader.</param>
        /// <param name="expandMultiTermQuery">if set to <c>true</c> [expand multi term query].</param>
        private void Init(Query query, String field, IndexReader reader, bool expandMultiTermQuery)
        {
            this.reader = reader;
            this.expandMultiTermQuery = expandMultiTermQuery;
            this.query = query;
            this.field = field;
        }

        /// <summary>
        /// Initializes the extractor.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <returns></returns>
        private TokenStream InitExtractor(TokenStream tokenStream)
        {
            WeightedSpanTermExtractor qse = defaultField == null
                                                ? new WeightedSpanTermExtractor()
                                                : new WeightedSpanTermExtractor(defaultField);

            qse.ExpandMultiTermQuery = expandMultiTermQuery;
            qse.SetWrapIfNotCachingTokenFilter(wrapToCaching);
            if (reader == null)
            {
                this.fieldWeightedSpanTerms = qse.GetWeightedSpanTerms(query,
                                                                       tokenStream, field);
            }
            else
            {
                this.fieldWeightedSpanTerms = qse.GetWeightedSpanTermsWithScores(query,
                                                                                 tokenStream, field, reader);
            }
            if (qse.IsCachedTokenStream)
            {
                return qse.TokenStream;
            }

            return null;
        }

        /// <summary>
        /// Starts the fragment.
        /// </summary>
        /// <param name="newFragment">The new fragment.</param>
        public void StartFragment(TextFragment newFragment)
        {
            foundTerms = new HashSet<string>();
            totalScore = 0;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is expand multi term query].
        /// </summary>
        /// <value>
        /// <c>true</c> if [is expand multi term query]; otherwise, <c>false</c>.
        /// </value>
        public bool IsExpandMultiTermQuery
        {
            get { return expandMultiTermQuery; }
            set { this.expandMultiTermQuery = value; }
        }

        /// <summary>
        /// Sets the wrap if not caching token filter.
        /// </summary>
        /// <param name="wrap">if set to <c>true</c> [wrap].</param>
        public void SetWrapIfNotCachingTokenFilter(bool wrap)
        {
            this.wrapToCaching = wrap;
        }

        #endregion
    }
}