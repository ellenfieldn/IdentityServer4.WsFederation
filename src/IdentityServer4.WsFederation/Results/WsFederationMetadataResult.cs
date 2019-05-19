// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation
{
    public class WsFederationMetadataResult : IEndpointResult
    {
        public string Metadata { get; }

        public WsFederationMetadataResult(string metadata) => Metadata = metadata;

        public Task ExecuteAsync(HttpContext context) => context.Response.WriteAsync(Metadata);
    }
}
