// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.WsFederation.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IdentityServer4.WsFederation.Tests.Extensions
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void AddQueryStringWithoutQ()
        {
            var baseUrl = "http://localhost/resource";
            var queryString = "param1=value1";
            var fullUrl = baseUrl.AddQueryString(queryString);
            Assert.AreEqual("http://localhost/resource?param1=value1", fullUrl);
        }

        [TestMethod]
        public void AddQueryStringExistingQsButNoAnd()
        {
            var baseUrl = "http://localhost/resource?existingParam=existingValue";
            var queryString = "param1=value1";
            var fullUrl = baseUrl.AddQueryString(queryString);
            Assert.AreEqual("http://localhost/resource?existingParam=existingValue&param1=value1", fullUrl);
        }

        [TestMethod]
        public void AddQueryStringTrailingAnd()
        {
            var baseUrl = "http://localhost/resource?existingParam=existingValue&";
            var queryString = "param1=value1";
            var fullUrl = baseUrl.AddQueryString(queryString);
            Assert.AreEqual("http://localhost/resource?existingParam=existingValue&param1=value1", fullUrl);
        }

        [TestMethod]
        public void AddQueryStringWithUrlNameValuePlainUrl()
        {
            var baseUrl = "http://localhost/resource";
            var fullUrl = baseUrl.AddQueryString("param1", "value1");
            Assert.AreEqual("http://localhost/resource?param1=value1", fullUrl);
        }

        [TestMethod]
        public void AddQueryStringWithUrlNameValuePlainUrlExistingQs()
        {
            var baseUrl = "http://localhost/resource?existingParam=existingValue";
            var fullUrl = baseUrl.AddQueryString("param1", "value1");
            Assert.AreEqual("http://localhost/resource?existingParam=existingValue&param1=value1", fullUrl);
        }

        [TestMethod]
        public void AddQueryStringWithUrlNameValuePlainUrlExistingQsTrailingAnd()
        {
            var baseUrl = "http://localhost/resource?existingParam=existingValue&";
            var fullUrl = baseUrl.AddQueryString("param1", "value1");
            Assert.AreEqual("http://localhost/resource?existingParam=existingValue&param1=value1", fullUrl);
        }

        [TestMethod]
        public void AddQueryStringWithUrlNameValueEncodesUrl()
        {
            var baseUrl = "http://localhost/resource";
            var fullUrl = baseUrl.AddQueryString("param1", "value+1 is good");
            Assert.AreEqual("http://localhost/resource?param1=value%2B1%20is%20good", fullUrl);
        }
    }
}
