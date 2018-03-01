using IdentityServer4.WsFederation.WsTrust.Entities;
using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IdentityServer4.WsFederation.Tests
{
    [TestClass]
    public class RstrSerializerTests
    {
        [TestMethod]
        public void SerializeTest()
        {
            var rstr = RequestSecurityTokenResponseSerializer.Deserialize(WResult_Saml2_Valid);
            var rstrString = RequestSecurityTokenResponseSerializer.Serialize(rstr);
            StringAssert.Contains(rstrString, "RequestSecurityTokenResponse");
            StringAssert.Contains(rstrString, "RequestedSecurityToken");
            StringAssert.Contains(rstrString, "Assertion");
        }

        [TestMethod]
        public void SignatureTest()
        {

        }

        /*protected virtual Saml2SecurityToken ValidateSignature(string token, TokenValidationParameters validationParameters)
        {
            var validatedSamlToken = validationParameters.SignatureValidator(token, validationParameters);
            var validatedSaml = validatedSamlToken as Saml2SecurityToken;
            return validatedSaml;
        }*/

        [TestMethod]
        public void DeserializeTest()
        {
            var rstr = RequestSecurityTokenResponseSerializer.Deserialize(WResult_Saml2_Valid);
            Assert.IsNotNull(rstr);

            Assert.IsNotNull(rstr.Lifetime);
            Assert.AreEqual("2017-04-23T16:11:17.348Z", rstr.Lifetime.Created);
            Assert.AreEqual("2017-04-23T17:11:17.348Z", rstr.Lifetime.Expires);

            Assert.IsNotNull(rstr.AppliesTo);
            Assert.IsNotNull(rstr.AppliesTo.EndpointReference);
            Assert.IsNotNull(rstr.AppliesTo.EndpointReference.Address);
            Assert.AreEqual("spn:fe78e0b4-6fe7-47e6-812c-fb75cee266a4", rstr.AppliesTo.EndpointReference.Address);

            Assert.AreEqual("http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0", rstr.TokenType);
            Assert.AreEqual("http://schemas.xmlsoap.org/ws/2005/02/trust/Issue", rstr.RequestType);
            Assert.AreEqual("http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey", rstr.KeyType);

            //TODO: Add the checks for the token deserialization
            Assert.IsNotNull(rstr.RequestedSecurityToken);
            Assert.AreEqual("https://sts.windows.net/add29489-7269-41f4-8841-b63c95564420/", rstr.RequestedSecurityToken.Issuer);

            var samlToken = rstr.RequestedSecurityToken as Saml2SecurityToken;
            Assert.IsNotNull(samlToken);
            Assert.AreEqual("_edc15efd-1117-4bf9-89da-28b1663fb890", samlToken.Assertion.Id.Value);
            Assert.AreEqual("https://sts.windows.net/add29489-7269-41f4-8841-b63c95564420/", samlToken.Assertion.Issuer.Value);
        }

        public static string WResult_Saml2_Valid
        {
            get => @"<t:RequestSecurityTokenResponse xmlns:t=""http://schemas.xmlsoap.org/ws/2005/02/trust""><t:Lifetime><wsu:Created xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">2017-04-23T16:11:17.348Z</wsu:Created><wsu:Expires xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">2017-04-23T17:11:17.348Z</wsu:Expires></t:Lifetime><wsp:AppliesTo xmlns:wsp=""http://schemas.xmlsoap.org/ws/2004/09/policy""><wsa:EndpointReference xmlns:wsa=""http://www.w3.org/2005/08/addressing""><wsa:Address>spn:fe78e0b4-6fe7-47e6-812c-fb75cee266a4</wsa:Address></wsa:EndpointReference></wsp:AppliesTo><t:RequestedSecurityToken><Assertion ID=""_edc15efd-1117-4bf9-89da-28b1663fb890"" IssueInstant=""2017-04-23T16:16:17.348Z"" Version=""2.0"" xmlns=""urn:oasis:names:tc:SAML:2.0:assertion""><Issuer>https://sts.windows.net/add29489-7269-41f4-8841-b63c95564420/</Issuer><Signature xmlns=""http://www.w3.org/2000/09/xmldsig#""><SignedInfo><CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#"" /><SignatureMethod Algorithm=""http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"" /><Reference URI=""#_edc15efd-1117-4bf9-89da-28b1663fb890""><Transforms><Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" /><Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#"" /></Transforms><DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" /><DigestValue>DO8QQoO629ApWPV3LiY2epQSv+I82iChybeRrXbhgtw=</DigestValue></Reference></SignedInfo><SignatureValue>O8JNyVKm9I7kMqlsaBgLCNwHA0qdXv34YHBVfg217lgeKkMC5taLU/EH7UeeMtapU6zMafcYoCH+Bp9zoqDpflgs78Hkjgn/dEUtjPFn7211VXClcTNqk+yhqXWtu6SKrabeIhKCKtoMA9lUAB4D6ABesb6MpwbM/ULq7T16tycZ3X//iXHeOiMwNiUAePYF22fmgrqRSDRHyLPtiLskP4UMksWJBrXUV96e9EU9aEciCvYpzMDv/VFUOCLiEkBqCdAtPVwVun+5eRk9zEh6qscWi0kAgFl3W3JhugcTTuGQYHXYVIHxbd5O33MwFIMUOmGrI1EXuk+cHIq2KUtSLg==</SignatureValue><KeyInfo><X509Data><X509Certificate>MIIDBTCCAe2gAwIBAgIQY4RNIR0dX6dBZggnkhCRoDANBgkqhkiG9w0BAQsFADAtMSswKQYDVQQDEyJhY2NvdW50cy5hY2Nlc3Njb250cm9sLndpbmRvd3MubmV0MB4XDTE3MDIxMzAwMDAwMFoXDTE5MDIxNDAwMDAwMFowLTErMCkGA1UEAxMiYWNjb3VudHMuYWNjZXNzY29udHJvbC53aW5kb3dzLm5ldDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAMBEizU1OJms31S/ry7iav/IICYVtQ2MRPhHhYknHImtU03sgVk1Xxub4GD7R15i9UWIGbzYSGKaUtGU9lP55wrfLpDjQjEgaXi4fE6mcZBwa9qc22is23B6R67KMcVyxyDWei+IP3sKmCcMX7Ibsg+ubZUpvKGxXZ27YgqFTPqCT2znD7K81YKfy+SVg3uW6epW114yZzClTQlarptYuE2mujxjZtx7ZUlwc9AhVi8CeiLwGO1wzTmpd/uctpner6oc335rvdJikNmc1cFKCK+2irew1bgUJHuN+LJA0y5iVXKvojiKZ2Ii7QKXn19Ssg1FoJ3x2NWA06wc0CnruLsCAwEAAaMhMB8wHQYDVR0OBBYEFDAr/HCMaGqmcDJa5oualVdWAEBEMA0GCSqGSIb3DQEBCwUAA4IBAQAiUke5mA86R/X4visjceUlv5jVzCn/SIq6Gm9/wCqtSxYvifRXxwNpQTOyvHhrY/IJLRUp2g9/fDELYd65t9Dp+N8SznhfB6/Cl7P7FRo99rIlj/q7JXa8UB/vLJPDlr+NREvAkMwUs1sDhL3kSuNBoxrbLC5Jo4es+juQLXd9HcRraE4U3UZVhUS2xqjFOfaGsCbJEqqkjihssruofaxdKT1CPzPMANfREFJznNzkpJt4H0aMDgVzq69NxZ7t1JiIuc43xRjeiixQMRGMi1mAB75fTyfFJ/rWQ5J/9kh0HMZVtHsqICBF1tHMTMIK5rwoweY0cuCIpN7A/zMOQtoD</X509Certificate></X509Data></KeyInfo></Signature><Subject><NameID Format=""urn:oasis:names:tc:SAML:2.0:nameid-format:persistent"">RrX3SPSxDw6z4KHaKB2V_mnv0G-LbRZdYvo1RQa1L7s</NameID><SubjectConfirmation Method=""urn:oasis:names:tc:SAML:2.0:cm:bearer"" /></Subject><Conditions NotBefore=""2017-04-23T16:11:17.348Z"" NotOnOrAfter=""2017-04-23T17:11:17.348Z""><AudienceRestriction><Audience>spn:fe78e0b4-6fe7-47e6-812c-fb75cee266a4</Audience></AudienceRestriction></Conditions><AttributeStatement><Attribute Name=""http://schemas.microsoft.com/identity/claims/tenantid""><AttributeValue>add29489-7269-41f4-8841-b63c95564420</AttributeValue></Attribute><Attribute Name=""http://schemas.microsoft.com/identity/claims/objectidentifier""><AttributeValue>d1ad9ce7-b322-4221-ab74-1e1011e1bbcb</AttributeValue></Attribute><Attribute Name=""http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name""><AttributeValue>User1@Cyrano.onmicrosoft.com</AttributeValue></Attribute><Attribute Name=""http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname""><AttributeValue>1</AttributeValue></Attribute><Attribute Name=""http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname""><AttributeValue>User</AttributeValue></Attribute><Attribute Name=""http://schemas.microsoft.com/identity/claims/displayname""><AttributeValue>User1</AttributeValue></Attribute><Attribute Name=""http://schemas.microsoft.com/identity/claims/identityprovider""><AttributeValue>https://sts.windows.net/add29489-7269-41f4-8841-b63c95564420/</AttributeValue></Attribute><Attribute Name=""http://schemas.microsoft.com/claims/authnmethodsreferences""><AttributeValue>http://schemas.microsoft.com/ws/2008/06/identity/authenticationmethod/password</AttributeValue></Attribute></AttributeStatement><AuthnStatement AuthnInstant=""2017-04-23T16:16:17.270Z""><AuthnContext><AuthnContextClassRef>urn:oasis:names:tc:SAML:2.0:ac:classes:Password</AuthnContextClassRef></AuthnContext></AuthnStatement></Assertion></t:RequestedSecurityToken><t:RequestedAttachedReference><SecurityTokenReference d3p1:TokenType=""http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0"" xmlns:d3p1=""http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd"" xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""><KeyIdentifier ValueType=""http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID"">_edc15efd-1117-4bf9-89da-28b1663fb890</KeyIdentifier></SecurityTokenReference></t:RequestedAttachedReference><t:RequestedUnattachedReference><SecurityTokenReference d3p1:TokenType=""http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0"" xmlns:d3p1=""http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd"" xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""><KeyIdentifier ValueType=""http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID"">_edc15efd-1117-4bf9-89da-28b1663fb890</KeyIdentifier></SecurityTokenReference></t:RequestedUnattachedReference><t:TokenType>http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0</t:TokenType><t:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</t:RequestType><t:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</t:KeyType></t:RequestSecurityTokenResponse>";
        }
    }
}
