// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using IdentityServer4.Quickstart.UI;
using IdentityServer4.WsFederation.Server.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer4.WsFederation.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var certificate = new X509Certificate2("IdentityServer4.WsFederation.Testing.pfx", "pw");
            var signingCredentials = new SigningCredentials(new X509SecurityKey(certificate), SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest);

            var builder = services.AddIdentityServer(options => 
            {
                options.IssuerUri = "urn:idsrv4:wsfed:server:sample";
            })
            .AddSigningCredential(signingCredentials)
            .AddTestUsers(TestUsers.Users)
            .AddInMemoryClients(Clients.TestClients)
            .AddInMemoryApiResources(new List<ApiResource>())
            .AddWsFederation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseIdentityServer();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
