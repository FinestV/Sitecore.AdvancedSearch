#region

using Sitecore.AdvancedSearch.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Search.Highlight;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Lucene;
using Sitecore.Diagnostics;
using Lucene.Net.Search;

#endregion

namespace Sitecore.AdvancedSearch.Highlighting
{
    /// <summary>
    /// Extends the search result item with highglighter functionality.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public class ExtendedSearchResults<TSource> where TSource : SearchResultItem
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedSearchResults{TSource}"/> class.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="query">The query.</param>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="context">The context.</param>
        public ExtendedSearchResults(SearchResults<TSource> results, Query query, Analyzer analyzer, IExecutionContext context)
        {
            Assert.ArgumentNotNull(results, "results");
            Assert.ArgumentNotNull(query, "query");
            Assert.ArgumentNotNull(analyzer, "analyzer");

            Query = query;
            Results = results;
            Analyzer = analyzer;
            ExecutionContext = context;
            NumberOfFragments = 3;
            FragmentLength = 80;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Defines the highlighted document type.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        public struct HighlightedDocument<TDocument>
        {
            private readonly TDocument document;
            private readonly Dictionary<string, string[]> highlights;

            public HighlightedDocument(TDocument document, Dictionary<string, string[]> highlights)
            {
                Assert.ArgumentNotNull(document, "document");
                Assert.ArgumentNotNull(highlights, "highlights");
                this.document = document;
                this.highlights = highlights;
            }

            public TDocument Document
            {
                get { return document; }
            }
            public Dictionary<string, string[]> Highlights
            {
                get { return highlights; }
            }
        }

        /// <summary>
        /// Gets or sets the query.
        /// </summary>
        /// <value>
        /// The query.
        /// </value>
        protected Query Query { get; set; }

        /// <summary>
        /// Gets or sets the analyzer.
        /// </summary>
        /// <value>
        /// The analyzer.
        /// </value>
        protected Analyzer Analyzer { get; set; }

        /// <summary>
        /// Gets or sets the execution context.
        /// </summary>
        /// <value>
        /// The execution context.
        /// </value>
        protected IExecutionContext ExecutionContext { get; set; }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>
        /// The results.
        /// </value>
        public SearchResults<TSource> Results { get; protected set; }

        /// <summary>
        /// Gets or sets the number of fragments.
        /// </summary>
        /// <value>
        /// The number of fragments.
        /// </value>
        public int NumberOfFragments { get; set; }

        /// <summary>
        /// Gets or sets the length of fragments.
        /// </summary>
        /// <value>
        /// The length of fragments
        /// </value>
        public int FragmentLength { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Highlights the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldNames">The field names.</param>
        /// <returns></returns>
        public HighlightedResults<TSource> Highlight(string fieldName, params string[] fieldNames)
        {
            var hits = this.Results.Hits.ToArray();

            var highlightedValues = new HighlightedResults<TSource>(hits.Length);
            if (hits.Length == 0)
            {
                return highlightedValues;
            }

            foreach (var hit in hits)
            {
                var doc = hit.Document;
                var highlights = HighlightDocument(fieldName, fieldNames, doc);
                highlightedValues.Add(doc, highlights);
            }

            return highlightedValues;
        }

        /// <summary>
        /// Highlights the field.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="startTag">The start tag.</param>
        /// <param name="endTag">The end tag.</param>
        /// <param name="fragmentLength">Length of the fragment.</param>
        /// <param name="numberOfFragments">The number of fragments.</param>
        /// <returns></returns>
        protected string[] HighlightField(string fieldName, string fieldValue, string startTag = "<strong>", string endTag = "</strong>", int fragmentLength = 150, int numberOfFragments = 1)
        {
            var scorer = new Lucene.Net.Search.Highlight.QueryScorer(Query);
            IFormatter formatter = new SimpleHTMLFormatter(startTag, endTag);
            var highlighter = new Highlighter(formatter, scorer)
                {
                    TextFragmenter = new SimpleFragmenter(fragmentLength)
                };
            var sr = new StringReader(fieldValue);
            var specificAnalyzer = GetAnalyzer(fieldName);
            TokenStream stream = specificAnalyzer.TokenStream(fieldName, sr);
            return highlighter.GetBestFragments(stream, fieldValue, numberOfFragments);
        }

        /// <summary>
        /// Highlights the document.
        /// </summary>
        /// <param name="fieldName1">The field name1.</param>
        /// <param name="fieldNames">The field names.</param>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        protected HighlightedDocument<TSource> HighlightDocument(string fieldName1, string[] fieldNames, TSource document)
        {
            var highlights = new Dictionary<string, string[]>(fieldNames.Length + 1);

            Action<string, string> processField = (string fieldName, string fieldValue) =>
                {
                    if (string.IsNullOrWhiteSpace(fieldValue))
                    {
                        highlights.Add(fieldName, new string[0]);
                    }
                    else
                    {
                        var highlightsForField = HighlightField(fieldName, fieldValue, fragmentLength: FragmentLength, numberOfFragments: NumberOfFragments);
                        highlights.Add(fieldName, highlightsForField);
                    }
                };



            var value = TryToGetValue(fieldName1, document);

            processField(fieldName1, value);

            foreach (var name in fieldNames)
            {
                value = TryToGetValue(name, document);
                processField(name, value);
            }

            return new HighlightedDocument<TSource>(document, highlights);
        }

        /// <summary>
        /// Tries to get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="doc">The document.</param>
        /// <returns></returns>
        protected string TryToGetValue(string key, TSource doc)
        {
            try
            {
                return doc[key];
            }
            catch (KeyNotFoundException)
            {
                // TODO Log warn
                return null;
            }
        }

        /// <summary>
        /// Gets the analyzer.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        protected Analyzer GetAnalyzer(string fieldName)
        {
            return Analyzer.GetAnalyzerByExecutionContext(new IExecutionContext[] { ExecutionContext, new FieldExecutionContext(fieldName) });
        }

        #endregion
    }
}