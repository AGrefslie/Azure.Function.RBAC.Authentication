namespace Azure.Function.RBAC.Authentication.Config
{
    public class AuthConfig
    {
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string RoleClaimType { get; set; } = string.Empty;
    }
}