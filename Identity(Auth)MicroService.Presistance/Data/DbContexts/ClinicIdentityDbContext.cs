using Identity_Auth_MicroService.Domain.Entities.IdenetityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Identity_Auth_MicroService.Presistance.Data.DbContexts
{
    public class ClinicIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public ClinicIdentityDbContext(DbContextOptions<ClinicIdentityDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            builder.Entity<ApplicationUser>()
                   .HasIndex(x => x.Email)
                   .IsUnique();

            builder.Entity<ApplicationUser>()
                   .HasIndex(x => x.PhoneNumber)
                   .IsUnique();

        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

    }
}
