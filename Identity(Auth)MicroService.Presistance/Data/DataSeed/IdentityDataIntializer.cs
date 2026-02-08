using Identity_Auth_MicroService.Domain.Contracts;
using Identity_Auth_MicroService.Domain.Entities.IdenetityModule;
using Identity_Auth_MicroService.Shared.CommonResult;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity_Auth_MicroService.Presistance.Data.DataSeed
{
    public class IdentityDataIntializer : IDataIntializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<IdentityDataIntializer> _logger;

        public IdentityDataIntializer(UserManager<ApplicationUser> userManager,
                                      RoleManager<IdentityRole> roleManager,
                                      ILogger<IdentityDataIntializer> logger)
        {
            _userManager=userManager;
            _roleManager=roleManager;
            _logger = logger;
        }
        public async Task IntializeAsync()
        {
            try
            {
                if (!_roleManager.Roles.Any())
                {
                    await _roleManager.CreateAsync(new IdentityRole("Clinic"));
                    await _roleManager.CreateAsync(new IdentityRole("Lab"));
                    await _roleManager.CreateAsync(new IdentityRole("Doctor"));
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));

                }
                if (await _userManager.Users.CountAsync() == 0)
                {
                    var User01 = new ApplicationUser
                    {
                        DisplayName ="ClinciAdmin",
                        Email="ClinciAdmin@gmail.com",
                        PhoneNumber="01002097078",
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true,
                        UserName = "ClinciAdmin"
                    };
                    var result = await _userManager.CreateAsync(User01, "P@ssw0rd");

                    await _userManager.AddToRoleAsync(User01, "Admin");
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError(error.Description);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while seeding identity data to the database => {ex}");

            }
        }
    }
}
