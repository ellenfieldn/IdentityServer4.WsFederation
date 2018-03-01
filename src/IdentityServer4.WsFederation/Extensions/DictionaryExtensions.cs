using System.Collections.Generic;
using System.Collections.Specialized;

namespace IdentityServer4.WsFederation.Extensions
{
    public static class DictionaryExtensions
    {
        public static NameValueCollection ToNameValueCollection(this IDictionary<string, string> dictionary)
        {
            var collection = new NameValueCollection();
            foreach(var pair in dictionary)
            {
                collection.Add(pair.Key, pair.Value);
            }
            return collection;
        }
    }
}
