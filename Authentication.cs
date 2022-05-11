using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Azure.Function.RBAC.Authentication.Config;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Azure.Function.RBAC.Authentication
{
    public class Authentication : IAuthentication
    {
        private readonly AuthConfig _authConfig;
        private JwtSecurityTokenHandler tokenHandler;

        public Authentication(IOptions<AuthConfig> authConfig)
        {
            _authConfig = authConfig.Value;
            tokenHandler = new JwtSecurityTokenHandler();
        }

        public async Task<bool> AuthenticateAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            string accessToken = GetAccessToken(requestMessage);
            if (string.IsNullOrEmpty(accessToken))
            {
                return false;
            }

            TokenValidationParameters validationParameters = await CreateValidationParameters(cancellationToken);

            var valid = ValidateToken(accessToken, validationParameters);

            return valid;
        }

        public async Task<bool> AuthenticateTokenWithRolesAsync(HttpRequestMessage requestMessage, string[] validRoles, CancellationToken cancellationToken)
        {
            try
            {
                string accessToken = GetAccessToken(requestMessage);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return false;
                }

                TokenValidationParameters validationParameters = await CreateValidationParameters(cancellationToken);

                var valid = ValidateTokenAndRole(accessToken, validationParameters, validRoles);

                return valid;
            }
            catch (Exception ex)
            {
                if (ex.Source == "System.Net.Http")
                {
                    return false;
                }

                throw;
            }
        }

        private bool ValidateToken(string accessToken, TokenValidationParameters validationParameters)
        {
            try
            {
                ClaimsPrincipal claimIdentity = tokenHandler.ValidateToken(accessToken, validationParameters, out _);

                if (claimIdentity != null)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                if (ex.Source == "Microsoft.IdentityModel.Tokens")
                {
                    return false;
                }

                throw;
            }
        }

        private bool ValidateTokenAndRole(string accessToken, TokenValidationParameters validationParameters, string[] validRoles)
        {
            try
            {
                ClaimsPrincipal claimIdentity = tokenHandler.ValidateToken(accessToken, validationParameters, out _);

                var roles = claimIdentity.FindAll(_authConfig.RoleClaimType)?.Select(s => s.Value);

                if (roles != null && roles.Intersect(validRoles).Count() > 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex) when (ex.Source == "Microsoft.IdentityModel.Tokens")
            {
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<TokenValidationParameters> CreateValidationParameters(CancellationToken cancellationToken)
        {
            try
            {
                var metadataAddress = $"https://sts.windows.net/{_authConfig.TenantId}/.well-known/openid-configuration";

                var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, new OpenIdConnectConfigurationRetriever());
                var config = await configManager.GetConfigurationAsync(cancellationToken);

                IList<string> validissuers = new List<string>()
                {
                    $"https://login.microsoftonline.com/{_authConfig.TenantId}/",
                    $"https://login.microsoftonline.com/{_authConfig.TenantId}/v2.0",
                    $"https://login.windows.net/{_authConfig.TenantId}/",
                    $"https://login.microsoft.com/{_authConfig.TenantId}/",
                    $"https://sts.windows.net/{_authConfig.TenantId}/"
                };

                var signingKeys = config.SigningKeys.ToList();

                var validationParameters = new TokenValidationParameters
                {
                    ValidAudience = _authConfig.ClientId,
                    ValidIssuers = validissuers,
                    IssuerSigningKeys = signingKeys,
                    ValidateLifetime = true,
                };

                return validationParameters;
            }
            catch (Exception ex) when (ex.Source == "Microsoft.IdentityModel.Protocols")
            {
                throw new Exception("Error fetching openid-configuration.");
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        private static string GetAccessToken(HttpRequestMessage request)
        {
            try
            {
                var authorizationHeaderValue = request.Headers.GetValues("Authorization").FirstOrDefault();
                
                string accessToken = string.Empty;

                if (!string.IsNullOrEmpty(authorizationHeaderValue))
                {
                    accessToken = authorizationHeaderValue.Replace("Bearer ", string.Empty);
                    return accessToken;
                }

                return accessToken;
            }
            catch (Exception ex) when (ex.Source == "System.Net.Http")
            {
                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            } 
        }
    }
}
