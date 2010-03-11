using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSlx.Lib.Utility.Impl
{
    /// <summary>
    /// Very simple cache service using a dictionary to implement the cache.
    /// (for testing purposes)
    /// </summary>
    public class MemCacheService : ICacheService
    {
        private Dictionary<String,object> _cache = new Dictionary<string,object>();

        #region ICacheService Members

        /// <summary>
        /// We provide a partial implementation - cache dependency and expiration are not honored.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheDependency"></param>
        /// <param name="absoluteExpiration"></param>
        /// <param name="slidingExpiration"></param>
        public void Insert(string key, object value, System.Web.Caching.CacheDependency cacheDependency, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            _cache[key] = value;
        }

        public object this[string index]
        {
            get
            {
                return _cache.ContainsKey(index) ? _cache[index] : null;
            }
            set
            {
                _cache[index] = value;
            }
        }

        #endregion
    }
}
