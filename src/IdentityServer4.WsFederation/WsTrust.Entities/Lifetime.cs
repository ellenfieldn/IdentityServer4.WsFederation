// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Xml.Serialization;

namespace IdentityServer4.WsFederation.WsTrust.Entities
{
    public class Lifetime
    {
        [XmlElement(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
        public DateTime? Created;
        [XmlElement(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
        public DateTime? Expires;

        public Lifetime()
        {
            Created = DateTime.UtcNow;
            Expires = DateTime.UtcNow.AddMinutes(5);
        }

        public Lifetime(DateTime? created, DateTime? expires)
        {
            if (created != null && expires != null && expires.Value <= created.Value)
            {
                throw new ArgumentException("Expires should be after created.");
            }

            Created = created?.ToUniversalTime();
            Expires = expires?.ToUniversalTime();
        }
    }
}