using ItreeNet.Data.Models.DB;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace ItreeNet.Middleware
{
    public class UserInfoClaims : IClaimsTransformation
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly ILogger _logger;

        public UserInfoClaims(IDbContextFactory<ZeiterfassungContext> dbFactory, ILogger logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // Clone current identity
            var clone = principal.Clone();

            if (clone == null || clone.Identity == null)
                throw new InvalidDataException("Principal or principal.identity is null");

            var newIdentity = (ClaimsIdentity)clone.Identity;

            var uid = principal.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;
            if (!Guid.TryParse(uid, out var azureId))
            {
                _logger.Warning("No valid uid claim found for authenticated user");
                return clone;
            }
            _logger.Debug($"Authenticated user: {uid}");

            await using var context = await _dbFactory.CreateDbContextAsync();

            var mitarbeiter = new TMitarbeiter();
            var canConnect = context.Database.CanConnect();
            if (canConnect)
            {
                // Get person
                mitarbeiter =
                    await context.TMitarbeiter.SingleOrDefaultAsync(m => m.AzureId == azureId);
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
