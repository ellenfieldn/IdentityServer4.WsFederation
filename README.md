# IdentityServer4.WsFederation
Plugin for IdentityServer4 that enables IdentityServer to function as a Ws-Federation Identity Provider.

[![Build status](https://ellenfieldn.visualstudio.com/IdentityServer4.WsFederation/_apis/build/status/IdentityServer4.WsFederation-CI)](https://ellenfieldn.visualstudio.com/IdentityServer4.WsFederation/_build/latest?definitionId=2)


## Getting Started
The easiest way to get started is to:
1. Clone the IdentityServer4.Quickstart project of your choice (unless you plan to build the UI from scratch)
1. Install the `IdentityServer4.Contrib.WsFederation` package.
1. Configure Startup.cs as below

Also, see the Samples folders for a working server (src/SampleServer) and client (src/SampleClient).

### The code
Do the following in `Startup.cs` to use this plugin with Asp.net Core.
If you don't use MVC, obviously take that out.
```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
    // If you don't manually specify the digest and signature algorithms, it'll fail.
    var certificate = new X509Certificate2("IdentityServer4.WsFederation.Testing.pfx", "pw");
    var signingCredentials = new SigningCredentials(new X509SecurityKey(certificate), SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest);

    var builder = services.AddIdentityServer(options => 
    {
        options.IssuerUri = "urn:idsrv4:wsfed:server:sample";
    })
    .AddSigningCredential(signingCredentials) // Must use this overload.
    .AddTestUsers(TestUsers.Users)
    .AddInMemoryClients(Clients.TestClients)
    .AddInMemoryApiResources(new List<ApiResource>())
    .AddWsFederation();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    //OtherStuff
    app.UseIdentityServer();
    // app.UseMvc.....
}
```


## Supported Functionality
Right now, this library is in alpha. Consequently, only a subset of the WsFederation standard is supported. It's also likely that some features of IdentityServer aren't well integrated yet.

### Supported Workflows
* Signin
* Metadata

### Supported Parameters
* wa
* wreply
* wctx
* wfresh

### Supported Outputs
* wa
* wresult
* wctx

## Something Vaguely Resembling a Roadmap
I plan to do this stuff. 

### Near-term features
These things are pretty much goals for the mid-term.
* Supporting the basic signout workflow.
* Supporting the rest of the request parameters
* Better input validation.
* Events.

### Long-term features
These will mostly correspond to the parts of WsFederation that were not mentioned above.
* Rst as an input parameter.
* Pointers to the incoming rst or outgoing rstr.
* Encryption?
* Different token types - SAML 1.1 vs SAML 2.0
