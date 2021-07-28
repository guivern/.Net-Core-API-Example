using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using RestApiBackend.Contracts.V1.Requests;
using RestApiBackend.Entities;
using RestApiBackend.Services;

namespace RestApiBackend.Data
{
    public static class Seed
    {
        public static async Task SeedUsersAsync(DataContext context, UserManager<User> userManager, IConfiguration configuration)
        {
            if (context.Users.Any()) return;

            var seedUsers = configuration.GetSection("SeedUsers").Get<List<RegisterRequest>>();
            
            foreach (var seed in seedUsers)
            {
                var newUser = new User {Email = seed.Email, UserName = seed.Username};
                var userRoles = new List<UserRole>();
                
                await userManager.CreateAsync(newUser, seed.Password);
                
                foreach (var roleId in seed.RolesIds)
                {
                    userRoles.Add(new UserRole {RoleId = roleId, UserId = newUser.Id});
                }

                await context.UserRoles.AddRangeAsync(userRoles);
                await context.SaveChangesAsync();
            }
        }
    }
}