using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using OpenSlx.Lib;
using OpenSlx.Lib.Utility;
using OpenSlx.Lib.Services;

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

namespace OpenSlx.Lib.Web.Utility
{
    /// <summary>
    /// Web-specific implementation of the cache service.
    /// This is a simple wrapper around the ASP.NET cache.
    /// </summary>
    public class WebCacheService : ICacheService
    {
        #region ICacheService Members

        /// <summary>
        /// Access cache item.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object this[string index]
        {
            get
            {
                return HttpContext.Current.Cache[index];                
            }
            set
            {
                if (value == null)
                    HttpContext.Current.Cache.Remove(index);
                else
                    HttpContext.Current.Cache[index] = value;
            }
        }

        /// <summary>
        /// Add an item.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheDependency"></param>
        /// <param name="absoluteExpiration"></param>
        /// <param name="slidingExpiration"></param>
        public void Insert(string key, object value, System.Web.Caching.CacheDependency cacheDependency, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            HttpContext.Current.Cache.Insert(key, value, cacheDependency, absoluteExpiration, slidingExpiration);
        }

        #endregion
    }
}
