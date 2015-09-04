#region

using System;
using System.Linq;
using System.Linq.Expressions;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Indexing;
using Sitecore.ContentSearch.Linq.Parsing;

#endregion

namespace Sitecore.AdvancedSearch.Highlighting
{
    /// <summary>
    /// Extends the Queryable result type.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    public class ExtendedGenericQueryable<TElement, TQuery> : GenericQueryable<TElement, TQuery>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedGenericQueryable{TElement, TQuery}"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="queryMapper">The query mapper.</param>
        /// <param name="queryOptimizer">The query optimizer.</param>
        /// <param name="fieldNameTranslator">The field name translator.</param>
        public ExtendedGenericQueryable(Index<TElement, TQuery> index, QueryMapper<TQuery> queryMapper, IQueryOptimizer queryOptimizer, FieldNameTranslator fieldNameTranslator)
            : base(index, queryMapper, queryOptimizer, fieldNameTranslator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedGenericQueryable{TElement, TQuery}"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="queryMapper">The query mapper.</param>
        /// <param name="queryOptimizer">The query optimizer.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="itemType">Type of the item.</param>
        /// <param name="fieldNameTranslator">The field name translator.</param>
        protected ExtendedGenericQueryable(Index<TQuery> index, QueryMapper<TQuery> queryMapper, IQueryOptimizer queryOptimizer, Expression expression, Type itemType, FieldNameTranslator fieldNameTranslator)
            : base(index, queryMapper, queryOptimizer, expression, itemType, fieldNameTranslator)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the specified expression.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public virtual TResult Execute<TResult>(Expression expression, out TQuery query)
        {
            query = this.GetQuery(expression);
            return this.Index.Execute<TResult>(query);
        }

        /// <summary>
        /// Gets the index of the execute.
        /// </summary>
        /// <value>
        /// The index of the execute.
        /// </value>
        public Index<TQuery> ExecuteIndex
        {
            get { return this.Index; }
        }

        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public override IQueryable<TDocument> CreateQuery<TDocument>(Expression expression)
        {
            GenericQueryable<TDocument, TQuery> queryable = new ExtendedGenericQueryable<TDocument, TQuery>(this.Index, this.QueryMapper, this.QueryOptimizer, expression, this.ItemType, this.FieldNameTranslator);
            ((IHasTraceWriter)queryable).TraceWriter = ((IHasTraceWriter)this).TraceWriter;
            return queryable;
        }

        #endregion
    }
}