using System.Xml.Serialization;

namespace IdentityServer4.WsFederation.WsTrust.Entities
{
    public class Lifetime
    {
        [XmlElement(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
        public string Created;
        [XmlElement(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
        public string Expires;
    }
}