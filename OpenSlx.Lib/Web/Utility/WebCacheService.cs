using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using OpenSlx.Lib;
using OpenSlx.Lib.Utility;

namespace SSSWorld.WebControls
{
    public class WebCacheService : ICacheService
    {
        #region ICacheService Members

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

        public void Insert(string key, object value, System.Web.Caching.CacheDependency cacheDependency, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            HttpContext.Current.Cache.Insert(key, value, cacheDependency, absoluteExpiration, slidingExpiration);
        }

        #endregion
    }
}
