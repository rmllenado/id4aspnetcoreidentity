using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Data
{
    public class IdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            UsersSeed(builder);
        }

        private void UsersSeed(ModelBuilder builder)
        {
            var password = "My long 123$ password";

            var alice = new ApplicationUser
            {
                Id = "1",
                UserName = "alice",
                NormalizedUserName = "ALICE",
                Email = "AliceSmith@email.com",
                NormalizedEmail = "AliceSmith@email.com".ToUpper(),
                EmailConfirmed = true
            };
            alice.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(alice, "alice");

            
            var bob = new ApplicationUser
            {
                Id = "2",
                UserName = "bob",
                NormalizedUserName = "BOB",
                Email = "BobSmith@email.com",
                NormalizedEmail = "bobsmith@email.com".ToUpper(),
                EmailConfirmed = true,
            };
            bob.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(bob, "bob");

            var mon = new ApplicationUser
            {
                Id = "3",
                UserName = "mon",
                NormalizedUserName = "MON",
                Email = "rmllenado@yahoo.com",
                NormalizedEmail = "rmllenado@yahoo.com".ToUpper(),
                EmailConfirmed = true,
                TwoFactorEnabled = true
            };
            mon.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(mon, "mon");


            builder.Entity<ApplicationUser>()
                .HasData(alice, bob, mon);


            builder.Entity<IdentityUserClaim<string>>()
                .HasData(
                    new IdentityUserClaim<string>
                    {
                        Id = 1,
                        UserId = "1",
                        ClaimType = "name",
                        ClaimValue = "Alice Smith"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 2,
                        UserId = "1",
                        ClaimType = "given_name",
                        ClaimValue = "Alice"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 3,
                        UserId = "1",
                        ClaimType = "family_name",
                        ClaimValue = "Smith"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 4,
                        UserId = "1",
                        ClaimType = "email",
                        ClaimValue = "AliceSmith@email.com"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 5,
                        UserId = "1",
                        ClaimType = "website",
                        ClaimValue = "http://alice.com"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 6,
                        UserId = "2",
                        ClaimType = "name",
                        ClaimValue = "Bob Smith"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 7,
                        UserId = "2",
                        ClaimType = "given_name",
                        ClaimValue = "Bob"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 8,
                        UserId = "2",
                        ClaimType = "family_name",
                        ClaimValue = "Smith"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 9,
                        UserId = "2",
                        ClaimType = "email",
                        ClaimValue = "BobSmith@email.com"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 10,
                        UserId = "2",
                        ClaimType = "website",
                        ClaimValue = "http://bob.com"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 11,
                        UserId = "1",
                        ClaimType = "email_verified",
                        ClaimValue = true.ToString()
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 12,
                        UserId = "2",
                        ClaimType = "email_verified",
                        ClaimValue = true.ToString()
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 13,
                        UserId = "1",
                        ClaimType = "address",
                        ClaimValue = @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 14,
                        UserId = "2",
                        ClaimType = "address",
                        ClaimValue = @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 15,
                        UserId = "1",
                        ClaimType = "location",
                        ClaimValue = "somewhere"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 16,
                        UserId = "3",
                        ClaimType = "name",
                        ClaimValue = "Mon Mon"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 17,
                        UserId = "3",
                        ClaimType = "given_name",
                        ClaimValue = "Mon"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 18,
                        UserId = "3",
                        ClaimType = "family_name",
                        ClaimValue = "Mon"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 19,
                        UserId = "3",
                        ClaimType = "email",
                        ClaimValue = "rmllenado@yahoo.com"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 20,
                        UserId = "3",
                        ClaimType = "website",
                        ClaimValue = "http://mon.com"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 21,
                        UserId = "3",
                        ClaimType = "address",
                        ClaimValue = @"{ 'street_address': 'Dehaven Drive', 'city': 'Yonkers', 'zip_code': 10703, 'state': 'NY', 'country': 'USA' }"
                    },
                    new IdentityUserClaim<string>
                    {
                        Id = 22,
                        UserId = "3",
                        ClaimType = "location",
                        ClaimValue = "somewhere"
                    });
        }
    }
}
