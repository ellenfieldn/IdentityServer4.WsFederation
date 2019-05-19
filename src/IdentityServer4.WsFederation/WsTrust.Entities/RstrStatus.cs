// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.WsFederation.WsTrust.Entities
{
    public class RstrStatus
    {
        public StatusCode Code;
    }

    public class StatusCode
    {
        private StatusCode(string value) => Value = value;

        public string Value { get; }

        public static StatusCode Valid { get => new StatusCode("http://docs.oasis-open.org/ws-sx/ws-trust/200512/status/valid"); }
        public static StatusCode Invalid { get => new StatusCode("http://docs.oasis-open.org/ws-sx/ws-trust/200512/status/invalid"); }
    }
}