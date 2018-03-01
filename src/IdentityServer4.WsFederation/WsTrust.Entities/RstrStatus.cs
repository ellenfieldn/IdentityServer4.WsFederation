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