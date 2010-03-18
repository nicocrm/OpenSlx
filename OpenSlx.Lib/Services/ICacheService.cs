using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;

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

namespace OpenSlx.Lib.Services
{
    /// <summary>
    /// Wrapper for cache provider service.
    /// There is an implementation using the ASP.NET cache in Web.Utility, and a 
    /// default implementation in Utility.Impl.
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
