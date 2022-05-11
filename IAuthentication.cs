using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Function.RBAC.Authentication
{
    public interface IAuthentication
    {
        /// <summary>
        /// Validates token by issuer, audince, signingkeys and lifetime. Returns true if token is valid, otherwise false.
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Boolean</returns>
        Task<bool> AuthenticateAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken);

        /// <summary>
        /// Validates token by issuer, audience, signingkeys, role and lifetime. Returns true if token is valid, otherwise false.
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <param name="validRoles"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Boolaen</returns>
        Task<bool> AuthenticateTokenWithRolesAsync(HttpRequestMessage requestMessage, string[] validRoles, CancellationToken cancellationToken);
    }
}
