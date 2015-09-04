#region

using Sitecore.AdvancedSearch.Models;
using System.Collections.Generic;

#endregion

namespace Sitecore.AdvancedSearch.Highlighting
{
    /// <summary>
    /// Defines the highlighted results type.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public class HighlightedResults<TSource> : Dictionary<TSource, ExtendedSearchResults<TSource>.HighlightedDocument<TSource>> where TSource : SearchResultItem
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightedResults{TSource}"/> class.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.</param>
        public HighlightedResults(int capacity)
            : base(capacity, new DocumentsComparator<TSource>())
        {
        }

        #endregion

        private class DocumentsComparator<TDocument> : IEqualityComparer<TDocument> where TDocument : SearchResultItem
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
            /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            public bool Equals(TDocument x, TDocument y)
            {
                return x.Uri.Equals(y.Uri);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public int GetHashCode(TDocument obj)
            {
                return obj.Uri.GetHashCode();
            }
        }
    }
}