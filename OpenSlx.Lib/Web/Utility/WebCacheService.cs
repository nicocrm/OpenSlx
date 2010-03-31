using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using OpenSlx.Lib;
using OpenSlx.Lib.Utility;
using OpenSlx.Lib.Services;

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
