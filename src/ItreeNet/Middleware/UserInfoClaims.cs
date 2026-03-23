using ItreeNet.Data.Models.DB;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace ItreeNet.Middleware
{
    public class UserInfoClaims : IClaimsTransformation
    {
        private readonly ZeiterfassungContext _context;
        private readonly ILogger _logger;

        public UserInfoClaims(ZeiterfassungContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // Clone current identity
            var clone = principal.Clone();

            if (clone == null || clone.Identity == null)
                throw new InvalidDataException("Principal or principal.identity is null");

            var newIdentity = (ClaimsIdentity)clone.Identity;

            var uid = principal.Claims.First(x => x.Type == "uid").Value;
            _logger.Debug($"Authenticated user: {uid}");

            var mitarbeiter = new TMitarbeiter();
            var canConnect = _context.Database.CanConnect();
            if (canConnect)
            {
                // Get person
                mitarbeiter =
                    await _context.TMitarbeiter.SingleOrDefaultAsync(m => m.AzureId == new Guid(uid));
            }

            if (canConnect && mitarbeiter != null)
            {
                // Add personId to claim
                newIdentity.AddClaim(new Claim("IsIntern", mitarbeiter.Intern.ToString()));
            }

            return await Task.FromResult(clone);
        }
    }
}
