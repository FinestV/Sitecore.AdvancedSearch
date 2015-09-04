#region

using Sitecore.ContentSearch.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucene.Net.Analysis;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Lucene;
using Sitecore.ContentSearch.LuceneProvider;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Diagnostics;
using Sitecore.AdvancedSearch.Models;

#endregion

namespace Sitecore.AdvancedSearch.Highlighting
{
    /// <summary>
    /// Highlighter extension methods.
    /// </summary>
    public static class MethodExtensions
    {
        #region Fields

        private static readonly MethodInfo getResultsMethod;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="MethodExtensions"/> class.
        /// </summary>
        static MethodExtensions()
        {
            getResultsMethod = typeof(QueryableExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(it => it.Name == "GetResults")
                .Single(it => it.GetParameters().Length == 1);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the extended queryable.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static IQueryable<TItem> GetExtendedQueryable<TItem>(this IProviderSearchContext context)
        {
            var luceneContext = context as LuceneSearchContext;
            Assert.IsNotNull(luceneContext, "Should be applied to the Lucene provide only...");

            var index = new ExtendedLinqToLuceneIndex<TItem>(luceneContext);
            if (ContentSearchConfigurationSettings.EnableSearchDebug)
            {
                (index as IHasTraceWriter).TraceWriter = new LoggingTraceWriter(SearchLog.Log);
            }

            return index.GetQueryable();
        }

        /// <summary>
        /// Gets the analyzer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">The index.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static Analyzer GetAnalyzer<T>(LuceneIndex<T> index, string fieldName)
        {
            return index.Parameters.Analyzer.GetAnalyzerByExecutionContext(new IExecutionContext[] { index.Parameters.ExecutionContext, new FieldExecutionContext(fieldName) });
        }

        /// <summary>
        /// Gets the extended results.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static ExtendedSearchResults<TSource> GetExtendedResults<TSource>(this IQueryable<TSource> source) where TSource : SearchResultItem
        {
            Assert.ArgumentNotNull(source, "source");
            var genQueryable = source.Provider as ExtendedGenericQueryable<TSource, LuceneQuery>;
            Assert.IsNotNull(genQueryable, "Can't get queryable...");
            var executeIndex = genQueryable.ExecuteIndex as LuceneIndex<TSource>;
            Analyzer analyzer = executeIndex.Parameters.Analyzer;
            IExecutionContext exContext = null;//Throws exception Sitecore 8 update 2. Pass null so field context is used to resolve Analyzer later: executeIndex.Parameters.ExecutionContext;
            LuceneQuery luceneQuery = null;

            SearchResults<TSource> results = genQueryable.Execute<SearchResults<TSource>>(Expression.Call(null, (getResultsMethod).MakeGenericMethod(new Type[] { typeof(TSource) }), new Expression[] { source.Expression }), out luceneQuery);
            return new ExtendedSearchResults<TSource>(results, luceneQuery.Query, analyzer, exContext);
        }

        #endregion
    }
}