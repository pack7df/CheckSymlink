using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Spt.Shared.Core.Models
{
    [Serializable]
    [DataContract]
    public class QueryResult<T>
    {
        [DataMember]
        public long Count { get; protected set; }

        [DataMember]
        public IEnumerable<T> Items { get; protected set; }

        [DataMember]
        public string ContinuationToken { get; protected set; }

        protected QueryResult()
        {

        }

        public QueryResult(IEnumerable<T> items, string continuationToken = null)
        {
            Items = items;
            Count = Items.Count();
            ContinuationToken = continuationToken;
        }

        public QueryResult(IEnumerable<T> items, long count, string continuationToken = null)
        {
            Items = items;
            Count = count;
            ContinuationToken = continuationToken;
        }
    }
}
