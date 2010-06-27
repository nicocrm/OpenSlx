using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;

/*
   OpenSlx - Open Source SalesLogix Library and Tools
   Copyright 2010 nicocrm (http://github.com/nicocrm/OpenSlx)

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

namespace OpenSlx.Lib.Services
{
    /// <summary>
    /// Wrapper for cache provider service.
    /// There is an implementation using the ASP.NET cache in Web.Utility, and a 
    /// default implementation in Services.Impl.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Retrieve cached item.  Return null if not available.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object this[String index] { get; set; }

        /// <summary>
        /// Insert cached item with options
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Object to be cached</param>
        /// <param name="cacheDependency">Dependency</param>
        /// <param name="absoluteExpiration">Date/Time at which the cache item will expire.  Pass NoAbsoluteExpiration if not desired.</param>
        /// <param name="slidingExpiration">The interval between the time the inserted object 
        /// is last accessed and the time at which that object expires. If this value is the equivalent of 20 minutes, 
        /// the object will expire and be removed from the cache 20 minutes after it was last accessed. 
        /// If you are using sliding expiration, the absoluteExpiration parameter must be NoAbsoluteExpiration.</param>
        void Insert(String key, object value, CacheDependency cacheDependency, DateTime absoluteExpiration, TimeSpan slidingExpiration);
    }
}
