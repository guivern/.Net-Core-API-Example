using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestApiBackend.Entities;

namespace RestApiBackend.Data
{
    public class DataContext : IdentityDbContext<User, Role, long, IdentityUserClaim<long>,
        UserRole, IdentityUserLogin<long>, IdentityRoleClaim<long>, IdentityUserToken<long>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        { }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region roles
            foreach (var role in (Role.RolesEnum[])Enum.GetValues(typeof(Role.RolesEnum)))
            {
                builder.Entity<Role>().HasData(new Role
                {
                    Id = (int) role,
                    Name = role.ToString(),
                });
            }
            #endregion

            builder.Entity<User>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            builder.Entity<Role>()
                .HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        }

        #region DbSets
        
        public virtual DbSet<RefreshToken> RefreshTokens {get;set;}
        
        #endregion
    }
}