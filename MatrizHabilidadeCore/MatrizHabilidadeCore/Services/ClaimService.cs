using MatrizHabilidadeCore.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MatrizHabilidadeCore.Services
{
    public class ClaimService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public ClaimService(IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<bool> AddUpdateClaim(Claims claims, string value)
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            var existingClaim = (await _userManager.GetClaimsAsync(user)).Where(c => c.Type == claims.Value).FirstOrDefault();

            if (existingClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, existingClaim);
            }

            await _userManager.AddClaimAsync(user, new Claim(claims.Value, value));

            return true;
        }

        public async Task<T> GetClaimValue<T>(Claims claims)
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            var claim = (await _userManager.GetClaimsAsync(user)).Where(c => c.Type == claims.Value).FirstOrDefault();

            if (claim == null)
            {
                return default;
            }

            return (T)Convert.ChangeType(claim.Value,typeof(T));
        }
    }
}
