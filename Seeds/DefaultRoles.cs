using Microsoft.AspNetCore.Identity;
using ShareIT.Constant;

namespace ShareIT.Seeds
{
    public class DefaultRoles
    {
     public static async Task SeedAsync(RoleManager<IdentityRole> roleManger)
        {
            if (!roleManger.Roles.Any())
            {
                await roleManger.CreateAsync(new IdentityRole(Roles.SuperAdmin.ToString()));
                await roleManger.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
                await roleManger.CreateAsync(new IdentityRole(Roles.Basic.ToString()));
                await roleManger.CreateAsync(new IdentityRole(Roles.Manager.ToString()));
                await roleManger.CreateAsync(new IdentityRole(Roles.Engineer.ToString()));
            }
        }
}
}
