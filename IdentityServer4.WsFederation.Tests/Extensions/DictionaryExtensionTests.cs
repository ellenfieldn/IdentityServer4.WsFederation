using IdentityServer4.WsFederation.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace IdentityServer4.WsFederation.Tests.Extensions
{
    [TestClass]
    public class DictionaryExtensionTests
    {
        [TestMethod]
        public void ToNameValueCollection_EmptyDictionary()
        {
            var myDictionary = new Dictionary<string, string>();
            var collection = myDictionary.ToNameValueCollection();
            Assert.IsNotNull(collection);
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void ToNameValueCollection_DictionarySize1()
        {
            var myDictionary = new Dictionary<string, string>
            {
                {"key1", "value1" }
            };
            var collection = myDictionary.ToNameValueCollection();
            Assert.IsNotNull(collection);
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("value1", collection.Get("key1"));
        }

        [TestMethod]
        public void ToNameValueCollection_DictionaryMultipleValues()
        {
            var myDictionary = new Dictionary<string, string>
            {
                {"key1", "value1" },
                {"key2", "value2" }
            };
            var collection = myDictionary.ToNameValueCollection();
            Assert.IsNotNull(collection);
            Assert.AreEqual(2, collection.Count);
            Assert.AreEqual("value1", collection.Get("key1"));
            Assert.AreEqual("value2", collection.Get("key2"));
        }
    }
}
