// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

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
