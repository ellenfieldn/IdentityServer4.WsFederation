using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml;
using Microsoft.IdentityModel.Tokens.Saml2;

namespace IdentityServer4.WsFederation.WsTrust.Entities
{
    public static class RequestSecurityTokenResponseSerializer
    {
        //TODO: Serialize the token
        public static string Serialize(RequestSecurityTokenResponse rstr)
        {
            var rstrString = SerializeBase(rstr);

            var handler = new Saml2SecurityTokenHandler();
            var token = handler.WriteToken(rstr.RequestedSecurityToken);
            var tokenElement = XElement.Parse(token, LoadOptions.None);

            XNamespace wstrust = "http://schemas.xmlsoap.org/ws/2005/02/trust";
            var requestedTokenElement = new XElement(wstrust + "RequestedSecurityToken");
            requestedTokenElement.Add(tokenElement);

            var doc = XDocument.Parse(rstrString);
            XNamespace policy = "http://schemas.xmlsoap.org/ws/2004/09/policy";
            var appliesTo = doc.Root.Element(policy + "AppliesTo");
            appliesTo.AddAfterSelf(requestedTokenElement);
            return doc.ToString(SaveOptions.DisableFormatting);
        }

        private static string SerializeBase(RequestSecurityTokenResponse rstr)
        {
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings();
            using (var writer = XmlWriter.Create(sb, settings))
            {
                var serializer = new XmlSerializer(typeof(RequestSecurityTokenResponse));
                serializer.Serialize(writer, rstr);
            }
            return sb.ToString();
        }

        public static RequestSecurityTokenResponse Deserialize(string rstrString)
        {
            var rstr = DeserializeBase(rstrString);
            rstr.RequestedSecurityToken = DeserializeToken(rstrString);
            return rstr;
        }

        private static RequestSecurityTokenResponse DeserializeBase(string rstrString)
        {
            using (var reader = new StringReader(rstrString))
            {
                var serializer = new XmlSerializer(typeof(RequestSecurityTokenResponse));
                return serializer.Deserialize(reader) as RequestSecurityTokenResponse;
            }
        }

        //TODO: Handle both SAML 1.1 and SAML 2.0
        private static SecurityToken DeserializeToken(string rstrString)
        {
            var doc = XDocument.Parse(rstrString);
            XNamespace wstrust = "http://schemas.xmlsoap.org/ws/2005/02/trust";
            var requestedTokenElement = doc.Root.Element(wstrust + "RequestedSecurityToken");
            XNamespace assertionNs = "urn:oasis:names:tc:SAML:2.0:assertion";
            var assertion = requestedTokenElement.Element(assertionNs + "Assertion");
            var handler = new Saml2SecurityTokenHandler();
            return handler.ReadSaml2Token(assertion.ToString());
        }
    }
}
