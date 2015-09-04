#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Lucene;
using Sitecore.ContentSearch.LuceneProvider;
using Sitecore.ContentSearch.LuceneProvider.Search;

//using Lucene.Net.Search;

#endregion

namespace Sitecore.AdvancedSearch.Highlighting
{
    /// <summary>
    /// Extends the Linq to Lucene with highglighter functionality.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class ExtendedLinqToLuceneIndex<TItem> : LinqToLuceneIndex<TItem>
    {
        #region Fields

        private static readonly MethodInfo applyScalarMethods;
        private static readonly MethodInfo applySearchMethods;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="ExtendedLinqToLuceneIndex{TItem}"/> class.
        /// </summary>
        static ExtendedLinqToLuceneIndex()
        {
            applyScalarMethods = typeof(LinqToLuceneIndex<TItem>).GetMethod("ApplyScalarMethods", BindingFlags.NonPublic | BindingFlags.Instance);
            applySearchMethods = typeof(LinqToLuceneIndex<TItem>).GetMethod("ApplySearchMethods", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedLinqToLuceneIndex{TItem}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ExtendedLinqToLuceneIndex(LuceneSearchContext context)
            : base(context)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedLinqToLuceneIndex{TItem}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="executionContext">The execution context.</param>
        public ExtendedLinqToLuceneIndex(LuceneSearchContext context, IExecutionContext executionContext)
            : base(context, executionContext)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the queryable.
        /// </summary>
        /// <returns></returns>
        public override IQueryable<TItem> GetQueryable()
        {
            IQueryable<TItem> queryable = new ExtendedGenericQueryable<TItem, LuceneQuery>(this, this.QueryMapper, this.QueryOptimizer, this.FieldNameTranslator);

            (queryable as IHasTraceWriter).TraceWriter = (this as IHasTraceWriter).TraceWriter;

            var filters = GetTypeInheritance(typeof(TItem)).SelectMany(t => t.GetCustomAttributes(typeof(IPredefinedQueryAttribute), true).Cast<IPredefinedQueryAttribute>()).ToList();

            foreach (IPredefinedQueryAttribute filter in filters)
            {
                queryable = filter.ApplyFilter(queryable, this.ValueFormatter);
            }

            return queryable;
        }

        /// <summary>
        /// Gets the type inheritance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private IEnumerable<Type> GetTypeInheritance(Type type)
        {
            yield return type;

            var baseType = type.BaseType;

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }

        /// <summary>
        /// Applies the scalar methods.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="processedResults">The processed results.</param>
        /// <param name="results">The results.</param>
        /// <returns></returns>
        private object ApplyScalarMethods<TResult, TDocument>(LuceneQuery query, object processedResults, TopDocs results)
        {
            return applyScalarMethods.MakeGenericMethod(new System.Type[] { typeof(TResult), typeof(TDocument) }).Invoke(this, new object[] { query, processedResults, results });
        }

        /// <summary>
        /// Applies the search methods.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="searchHits">The search hits.</param>
        /// <returns></returns>
        private object ApplySearchMethods<TElement>(LuceneQuery query, TopDocs searchHits)
        {
            var genMethod = applySearchMethods.MakeGenericMethod(new Type[] { typeof(TElement) });
            return genMethod.Invoke(this, new object[] { query, searchHits });
        }

        #endregion
    }
}