using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace ePaperLive.Models
{
    /// <summary>
    /// Used to temporarily store data
    /// in between functions of the app.
    /// </summary>
    public class TempData
    {
        public MemoryCache Cache = MemoryCache.Default;
        private readonly CacheItemPolicy CachePolicy = new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(180) };
        private DateTimeOffset Expiration = DateTimeOffset.Now.AddMinutes(double.Parse(ConfigurationManager.AppSettings["cache_expiry"]));

        public TempData()
        {
            if (!Cache.Any(c => c.Key == "registered"))
            {
                Cache.Add("registered", false, CachePolicy);
            }
        }

        public object Get(string key)
        {
            return Cache.Get(key);
        }

        /// <summary>
        /// Adds an item to the cache
        /// </summary>
        /// <param name="key">Identifier</param>
        /// <param name="value">Value of identifier.</param>
        /// <param name="expiration">Determines how long item should be cached.</param>
        public void Set(string key, object value)
        {
            CachePolicy.AbsoluteExpiration = Expiration;
            // Don’t add twice.
            if (!Cache.Any(c => c.Key == key))
            {
                Cache.Add(key, value, CachePolicy);
            }
        }
    }
}