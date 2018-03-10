using System.Xml.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.WsFederation.WsTrust.Entities
{
    [XmlRoot(ElementName ="RequestSecurityTokenResponse", Namespace = "http://schemas.xmlsoap.org/ws/2005/02/trust")]
    public class RequestSecurityTokenResponse
    {
        //minOccurs 0
        //Uri
        public string TokenType;

        //Uri
        public string RequestType;

        //minOccurs 0
        [XmlIgnore]
        public SecurityToken RequestedSecurityToken;

        //minOccurs 0
        [XmlElement(Namespace = "http://schemas.xmlsoap.org/ws/2004/09/policy")]
        public AppliesTo AppliesTo;

        //minOccurs 0
        public Lifetime Lifetime;

        //minOccurs0
        //KeyType
        public string KeyType;

        //minOccurs 0
        //StatusCode
        //public RstrStatus Status;

        //minOccurs 0
        //public EndpointReference Issuer;

        //<xs:element ref='wst:RequestedAttachedReference' minOccurs='0' />
        //<xs:element ref='wst:RequestedUnattachedReference' minOccurs='0' />
        //<xs:element ref='wst:RequestedProofToken' minOccurs='0' />
        //<xs:element ref='wst:Entropy' minOccurs='0' />
        //<xs:element ref='wst:AllowPostdating' minOccurs='0' />
        //<xs:element ref='wst:Renewing' minOccurs='0' />
        //<xs:element ref='wst:OnBehalfOf' minOccurs='0' />
        //<xs:element ref='wst:AuthenticationType' minOccurs='0' />
        //<xs:element ref='wst:Authenticator' minOccurs='0' />
        //<xs:element ref='wst:KeySize' minOccurs='0' />
        //<xs:element ref='wst:SignatureAlgorithm' minOccurs='0' />
        //<xs:element ref='wst:Encryption' minOccurs='0' />
        //<xs:element ref='wst:EncryptionAlgorithm' minOccurs='0' />
        //<xs:element ref='wst:CanonicalizationAlgorithm' minOccurs='0' />
        //<xs:element ref='wst:ProofEncryption' minOccurs='0' />
        //<xs:element ref='wst:UseKey' minOccurs='0' />
        //<xs:element ref='wst:SignWith' minOccurs='0' />
        //<xs:element ref='wst:EncryptWith' minOccurs='0' />
        //<xs:element ref='wst:DelegateTo' minOccurs='0' />
        //<xs:element ref='wst:Forwardable' minOccurs='0' />
        //<xs:element ref='wst:Delegatable' minOccurs='0' />
        //<xs:element ref='wsp:Policy' minOccurs='0' />
        //<xs:element ref='wsp:PolicyReference' minOccurs='0' />
        //<xs:any namespace='##other' processContents='lax' minOccurs='0' maxOccurs='unbounded' />
        //<xs:attribute name = "Context" type="xs:anyURI" use="optional"/>
    }
}
