using ASC.Model.BaseTypes;
using ASC.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ASC.Web.Data
{
    public class IdentitySeed : IIdentitySeed
    {
        private readonly IConfiguration _configuration;

        public IdentitySeed(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration is null");
        }

        public async Task Seed(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (roleManager == null) throw new ArgumentNullException(nameof(roleManager), "RoleManager is null");
            if (userManager == null) throw new ArgumentNullException(nameof(userManager), "UserManager is null");

            var roles = _configuration.GetSection("ApplicationSettings:Roles").Get<string[]>() ?? Array.Empty<string>();

            if (roles.Length == 0)
                throw new InvalidOperationException("Roles configuration is missing or empty in ApplicationSettings.");

            // Tạo Roles nếu chưa tồn tại
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole { Name = role });
                }
            }

            await CreateUserIfNotExists(userManager, "Admin", "AdminEmail", "AdminPassword", Roles.Admin.ToString(), new List<Claim>
            {
                new Claim(ClaimTypes.Role, Roles.Admin.ToString())
            });

            await CreateUserIfNotExists(userManager, "Engineer", "EngineerEmail", "EngineerPassword", Roles.Engineer.ToString(), new List<Claim>
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", _configuration[$"ApplicationSettings:EngineerEmail"] ?? ""),
                new Claim("IsActive", "True")
            });
        }

        private async Task CreateUserIfNotExists(UserManager<IdentityUser> userManager, string userKey, string emailKey, string passwordKey, string role, List<Claim> claims)
        {
            var email = _configuration[$"ApplicationSettings:{emailKey}"];
            var name = _configuration[$"ApplicationSettings:{userKey}Name"];
            var password = _configuration[$"ApplicationSettings:{passwordKey}"];

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return;

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = name,
                    Email = email,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    await userManager.AddClaimsAsync(user, claims);
                }
            }
        }
    }
}
