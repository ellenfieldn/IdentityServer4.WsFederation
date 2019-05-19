// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Xml.Serialization;

namespace IdentityServer4.WsFederation.WsTrust.Entities
{
    public class AppliesTo
    {
        public AppliesTo() { }

        public AppliesTo(string address)
        {
            EndpointReference = new EndpointReference
            {
                Address = address
            };
        }

        [XmlElement(Namespace = "http://www.w3.org/2005/08/addressing")]
        public EndpointReference EndpointReference;
    }
}