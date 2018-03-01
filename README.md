# IdentityServer4.WsFederation
Full .Net Core implementation of WsFederation for IdentityServer4 and asp.net core. 

I built this because:
* Everything else i saw was targetted at .Net Framework
* I wanted to be able to use WsFederation while targetting .net core.
* I want it to be possible to deploy linux containers with a WsFederation Identity Provider.

## Getting Started
If you want to get started, see the Samples folders for a working server and client. Long-term, the easiest way to get started will be to clone the IdentityServer4.Quickstart project of your choice and then to add a nuget with a reference to the IdentityServer4.WsFederation plugin.

For now, it's probably easiest to just clone my this repo and either:
1. Modify the Sample server project to fit your needs
2. Create a new project, clone an IdentityServer4.Quickstart project, use the Startup.cs file in the Sample Server project.

### The code
Do the following in `Startup.cs` to use this plugin with Asp.net Core.
If you don't use MVC, obviously take that out.
```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
    services.AddAuthentication(sharedOptions =>
    {
        sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
    })
    .AddWsFederation(options =>
    {
        options.Wtrealm = "urn:idsrv4:wsfed:sample";
        options.MetadataAddress = "http://localhost:63338/wsfederation/metadata";
        options.RequireHttpsMetadata = false;
        options.Wreply = "http://localhost:63307/signin-wsfed";
    })
    .AddCookie();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    // Other stuff abbreviated
    app.UseAuthentication();
    // app.UseMvc....
}
```


## Supported Functionality
Right now, this is basically in the POC state and is my first pass at getting a working IdentityServer4 plugin to support WsFederation for passive authentication.

### Supported Workflows
* Signin
* Metadata

### Supported Parameters
* wa
* wreply
* wctx

### Supported Outputs
* wa
* wresult
* wctx

### Top Priority Things
These are the things that I'm planning on doing pretty soon.
* I'm fairly certain I am doing Bad Stuff&trade; with the way I did some of the IdentityServer4-specific things.
* Need to double check the way I did the asp.net core extensions. 
* Need to make cookies configurable.
* Need to at least make an effort to organize the files.
* Need to figure out how I want to handle the awkwardness with the SigningCredentials. Short run, I can probably get a better solution. Long run, I think IdentityServer4 should probably make some small changes to their extensions. Need to understand better before I file an issue and potentially offer to "fix" it.

### Other things that are on the roadmap
These things are pretty much goals for the mid-term.
* Supporting the basic signout workflow.
* Supporting the rest of the easy parameters.
* Better input validation.
* Nuget package.
* Logging.

### Long-term features
These will mostly correspond to the parts of WsFederation that were not mentioned above.
* Rst as an input parameter.
* Pointers to the incoming rst or outgoing rstr.
* Encryption?
