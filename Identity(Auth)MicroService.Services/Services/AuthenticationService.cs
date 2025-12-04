using Identity_Auth_MicroService.Domain.Entities.IdenetityModule;
using Identity_Auth_MicroService.Services_Abstraction.Interfaces;
using Identity_Auth_MicroService.Shared.CommonResult;
using Identity_Auth_MicroService.Shared.IdentityDTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Identity_Auth_MicroService.Services.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configration;

        public AuthenticationService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configration = configuration;
        }


        public async Task<Result<LoginReturnedDataDTO>> LoginAsync(LoginDTO loginDTO)
        {
            var User = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (User == null)
                return Error.InvalidCredentials("User.InvalidCredentials");

            var CheckPassword = await _userManager.CheckPasswordAsync(User, loginDTO.Password);
            if (!CheckPassword)
                return Error.InvalidCredentials("User.InvalidCredentials");

            var Token = await CreateTokenAsync(User);
            return new LoginReturnedDataDTO(User.DisplayName, User.Email!, Token);

        }

        public async Task<Result<UserDTO>> RegisterAsync(RegisterDTO registerDTO)
        {
            var User = new ApplicationUser
            {
                UserName = registerDTO.DisplayName.Replace(" ", ""),
                DisplayName = registerDTO.DisplayName,
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber
            };
            var IdentityResult = await _userManager.CreateAsync(User, registerDTO.Password);

            if (IdentityResult.Succeeded)
            {
                var Token = await CreateTokenAsync(User);
                return new UserDTO(User.DisplayName, User.Email!);
            }

            return IdentityResult.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
        }

        private async Task<string> CreateTokenAsync(ApplicationUser user)
        {
            // Token [Issuer, Audience, Claims, Expires, SigningCredentials]

            var Claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Name, user.DisplayName!)
            };

            var Roles = await _userManager.GetRolesAsync(user);
            foreach (var role in Roles)
            {
                Claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var Scuritykey = _configration["JWTOptions:SecretKey"];
            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Scuritykey!));
            var Cred = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);

            var Token = new JwtSecurityToken(
                issuer: _configration["JWTOptions:Issuer"],
                audience: _configration["JWTOptions:Audience"],
                expires: DateTime.Now.AddHours(1),
                claims: Claims,
                signingCredentials: Cred
                );

            return new JwtSecurityTokenHandler().WriteToken(Token);


        }
        public async Task<bool> CheckEmailAsync(string Email)
        {
            var User = await _userManager.FindByEmailAsync(Email);
            return User != null;
        }

        public async Task<Result<LoginReturnedDataDTO>> GetUserByEmailAsync(string Email)
        {
            var User = await _userManager.FindByEmailAsync(Email);
            if (User == null)
                return Error.NotFound("User.NotFound", $"No User With This Email {Email} Was Found");

            return new LoginReturnedDataDTO(User.Email!, User.DisplayName, await CreateTokenAsync(User));
        }
    }
}
