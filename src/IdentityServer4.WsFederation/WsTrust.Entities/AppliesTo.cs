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