using Identity_Auth_MicroService.Domain.Contracts;
using Identity_Auth_MicroService.Domain.Entities.IdenetityModule;
using Identity_Auth_MicroService.Services_Abstraction.Interfaces;
using Identity_Auth_MicroService.Servives_Abstraction.Interfaces;
using Identity_Auth_MicroService.Shared.CommonResult;
using Identity_Auth_MicroService.Shared.IdentityDTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity_Auth_MicroService.Services.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClinicClient _clinicClient;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            IClinicClient clinicClient)
        {
            _userManager = userManager;
            _configration = configuration;
            _unitOfWork = unitOfWork;
            _clinicClient = clinicClient;
        }

        public async Task<Result<LoginReturnedDataDTO>> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
                return Error.InvalidCredentials("User.InvalidCredentials");

            var checkPassword = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
            if (!checkPassword)
                return Error.InvalidCredentials("User.InvalidCredentials");

            if (await _userManager.IsInRoleAsync(user, "Doctor"))
            {
                var isActive = await _clinicClient.IsDoctorActiveAsync(user.Id); // user.Id = IdentityUserId
                if (!isActive)
                    return Error.Validation("Doctor.Inactive", "Your account is not active. Please contact admin.");
            }

            var accessToken = await CreateTokenAsync(user);
            var (refreshToken, refreshExp) = await CreateRefreshTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new LoginReturnedDataDTO(user.DisplayName, user.Email!, accessToken, refreshToken, refreshExp,roles);
        }

        public async Task<Result<UserDTO>> RegisterAsync(RegisterDTO registerDTO)
        {
            // Check Email
            var existingEmail = await _userManager.FindByEmailAsync(registerDTO.Email);

            if (existingEmail is not null)
                return Error.Validation("EmailExists", "Email already exists");

            // Check Phone Number
            var existingPhone = await _userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == registerDTO.PhoneNumber);

            if (existingPhone is not null)
                return Error.Validation("PhoneExists", "Phone number already exists");

            var user = new ApplicationUser
            {
                UserName = registerDTO.DisplayName.Replace(" ", ""),
                DisplayName = registerDTO.DisplayName,
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber
            };

            var identityResult = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!identityResult.Succeeded)
                return identityResult.Errors
                    .Select(e => Error.Validation(e.Code, e.Description))
                    .ToList();

            var addToRoleResult = await _userManager.AddToRoleAsync(user, registerDTO.role!);

            if (!addToRoleResult.Succeeded)
                return addToRoleResult.Errors
                    .Select(e => Error.Validation(e.Code, e.Description))
                    .ToList();

            return new UserDTO(user.Id, user.DisplayName, user.Email!);
        }

        public async Task<bool> CheckEmailAsync(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            return user != null;
        }

        public async Task<Result<LoginReturnedDataDTO>> GetUserByEmailAsync(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
                return Error.NotFound("User.NotFound", $"No User With This Email {Email} Was Found");

            var accessToken = await CreateTokenAsync(user);

            // لو DTO بتاعك لازم RefreshToken، عندك خيارين:
            // 1) تعمل DTO تاني لـ CurrentUser
            // 2) ترجع refreshToken فاضي (مش بحبه)
            // الأفضل: خليك تستعمل CurrentUserDto منفصل.
            // لكن بما إنك مصمم على LoginReturnedDataDTO:
            // هنعمل refresh جديد هنا (مش مُفضّل). شوف الخيار تحت.

            var (refreshToken, refreshExp) = await CreateRefreshTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            return new LoginReturnedDataDTO(user.DisplayName, user.Email!, accessToken, refreshToken, refreshExp, roles);
        }

        public async Task<Result<bool>> DeleteUserByEmailAsync(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
                return Result<bool>.Fail(
                    Error.NotFound("User.NotFound", $"No User With This Email {Email} Was Found")
                );

            if (user.Email == "ClinicAdmin@gmail.com")
                return Result<bool>.Fail(
                    Error.Forbidden("User.Forbidden", "Cannot delete admin user")
                );

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return Result<bool>.Fail(
                    result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList()
                );

            return Result<bool>.Ok(true);
        }


        public async Task<Result<LoginReturnedDataDTO>> RefreshAsync(RefreshRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return Error.Unauthorized("Token.InvalidRefresh", "Missing refresh token");

            var hash = RefreshTokenHelper.Hash(dto.RefreshToken);
            var repo = _unitOfWork.GetRepository<RefreshToken>();

            var stored = await repo.FirstOrDefaultAsync(x => x.TokenHash == hash);

            if (stored == null || stored.RevokedAt != null || stored.ExpiresAt <= DateTime.UtcNow)
                return Error.Unauthorized("Token.InvalidRefresh", "Invalid or expired refresh token");

            var user = await _userManager.FindByIdAsync(stored.UserId);
            if (user == null)
                return Error.Unauthorized("User.NotFound", "User for this token not found");

            // revoke old
            stored.RevokedAt = DateTime.UtcNow;

            // rotate
            var newRefresh = RefreshTokenHelper.Generate();
            var newHash = RefreshTokenHelper.Hash(newRefresh);
            var newExp = DateTime.UtcNow.AddDays(14);

            stored.ReplacedByTokenHash = newHash;

            await repo.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = newHash,
                ExpiresAt = newExp,
                CreatedAt = DateTime.UtcNow
            });

            repo.Update(stored);
            await _unitOfWork.SaveChangesAsync();

            var newAccess = await CreateTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            return new LoginReturnedDataDTO(user.DisplayName, user.Email!, newAccess, newRefresh, newExp, roles);
        }

        private async Task<(string refreshToken, DateTime expiresAt)> CreateRefreshTokenAsync(ApplicationUser user)
        {
            var repo = _unitOfWork.GetRepository<RefreshToken>();

            // ✅ revoke active tokens للمستخدم ده فقط (أداء أحسن من GetAll)
            var activeTokens = await repo.ListAsync(t =>
                t.UserId == user.Id &&
                t.RevokedAt == null &&
                t.ExpiresAt > DateTime.UtcNow);

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                repo.Update(token);
            }

            var refreshToken = RefreshTokenHelper.Generate();
            var tokenHash = RefreshTokenHelper.Hash(refreshToken);
            var expiresAt = DateTime.UtcNow.AddDays(14);

            await repo.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();
            return (refreshToken, expiresAt);
        }

        private async Task<string> CreateTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
                {
                    // 🔹 Standard Claims
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id), // مهم جدًا
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                    new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),

                    // 🔹 ASP.NET Identity Compatible Claims
                    new Claim(ClaimTypes.NameIdentifier, user.Id),   // مهم جدًا للـ Owner check
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(ClaimTypes.Name, user.DisplayName!)
                };

            // 🔹 Roles
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var secretKey = _configration["JWTOptions:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configration["JWTOptions:Issuer"],
                audience: _configration["JWTOptions:Audience"],
                expires: DateTime.UtcNow.AddHours(1),
                claims: claims,
                signingCredentials: cred
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Result<bool>> LogoutAsync(LogoutRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return Error.Validation("Token.Missing", "Refresh token is required");

            var hash = RefreshTokenHelper.Hash(dto.RefreshToken);
            var repo = _unitOfWork.GetRepository<RefreshToken>();

            var stored = await repo.FirstOrDefaultAsync(x => x.TokenHash == hash);
            if (stored == null)
                return true; // اعتبره logout anyway

            if (stored.RevokedAt == null)
            {
                stored.RevokedAt = DateTime.UtcNow;
                repo.Update(stored);
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        public async Task<Result<UserDTO>> UpdateUserAsync(string userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Error.NotFound("User.NotFound", $"No User With This Id {userId} Was Found");

            // =========================
            // Email (only if sent & changed)
            // =========================
            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userManager.FindByEmailAsync(dto.Email);
                if (existing != null && existing.Id != user.Id)
                    return Error.Validation("User.EmailExists", "Email is already used by another user");

                var setEmail = await _userManager.SetEmailAsync(user, dto.Email);
                if (!setEmail.Succeeded)
                    return setEmail.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();

                var setUserName = await _userManager.SetUserNameAsync(user, dto.Email);
                if (!setUserName.Succeeded)
                    return setUserName.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            }

            // =========================
            // DisplayName (only if sent)
            // =========================
            if (!string.IsNullOrWhiteSpace(dto.DisplayName))
                user.DisplayName = dto.DisplayName;

            // =========================
            // Phone (only if sent)
            // =========================
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            // لو مفيش أي تغيير في بيانات البروفايل… ممكن تتجنب UpdateAsync (اختياري)
            // بس خلينا نعمل UpdateAsync عادي
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();

            //// =========================
            //// Password (only if sent)
            //// =========================
            //if (!string.IsNullOrWhiteSpace(dto.Password))
            //{
            //    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            //    var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, dto.Password);

            //    if (!resetResult.Succeeded)
            //        return resetResult.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            //}

            return new UserDTO(user.Id, user.DisplayName, user.Email!);
        }

        public async Task<Result<bool>> UpdatePassword(string userId, UpdatePasswordDto password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Error.NotFound("User.NotFound", $"No User With This Id {userId} Was Found");
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, password.NewPassword);
            if (!resetResult.Succeeded)
                return resetResult.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            return true;
        }

        public async Task<Result<List<UserDTO>>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var userDtos = new List<UserDTO>();
            foreach (var user in users)
            {
                userDtos.Add(new UserDTO(user.Id, user.DisplayName, user.Email!));
            }
            return await Task.FromResult(userDtos);
        }

        public async Task<Result<ReturnUserDataDTO>> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Error.NotFound("User.NotFound", $"No User With This Id {userId} Was Found");
            return new ReturnUserDataDTO(user.DisplayName, user.Email!, user.PhoneNumber!);
        }


    }
}
