using System;
using System.Collections.Generic;
using System.Text;

/*
   OpenSlx - Open Source SalesLogix Library and Tools
   Copyright 2010 nicocrm (http://github.com/ngaller/OpenSlx)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace OpenSlx.Lib.Services.Impl
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

        /// <summary>
        /// Retrieve cache item
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
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
