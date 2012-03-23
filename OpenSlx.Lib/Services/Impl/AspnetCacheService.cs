using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace OpenSlx.Lib.Services.Impl
{
    /// <summary>
    /// Implementation of ICacheService using the default ASP.NET cache.
    /// For convenience during testing if HttpContext.Current is not valid all calls are ignored.
    /// </summary>
    public class AspnetCacheService : ICacheService
    {
        public object this[string index]
        {
            get
            {
                return HttpContext.Current == null ? null : HttpContext.Current.Cache[index];
            }
            set
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Cache[index] = value;
            }
        }

        public void Insert(string key, object value, System.Web.Caching.CacheDependency cacheDependency, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Cache.Add(key, value, cacheDependency, absoluteExpiration, slidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
            }
        }
    }
}
