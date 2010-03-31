using System;
using System.Collections.Generic;
using System.Text;

/*
    OpenSlx - Open Source SalesLogix Library and Tools
    Copyright (C) 2010 Strategic Sales Systems

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
