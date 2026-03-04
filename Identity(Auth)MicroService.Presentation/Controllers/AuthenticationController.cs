using Identity_Auth_MicroService.Shared.CommonResult;
using Identity_Auth_MicroService.Shared.IdentityDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using MyIAuthService = Identity_Auth_MicroService.Services_Abstraction.Interfaces.IAuthenticationService;

namespace Identity_Auth_MicroService.Presentation.Controllers
{
    [ApiController]
    [Route("Clinic/[controller]")]
    public class AuthenticationController : ApiBaseController
    {
        private readonly MyIAuthService _authenticationService;

        public AuthenticationController(MyIAuthService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        // ✅ Public
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var result = await _authenticationService.LoginAsync(loginDTO);
            return HandleResult(result);
        }

        // ✅ Public (لو انت هتمنع التسجيل العام خليها Admin أو Internal بس)
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var result = await _authenticationService.RegisterAsync(registerDTO);
            return HandleResult(result);
        }

        // ✅ Public
        [AllowAnonymous]
        [HttpGet("EmailExists")]
        public async Task<ActionResult<bool>> CheckEmail([FromQuery] string email)
        {
            var exists = await _authenticationService.CheckEmailAsync(email);
            return Ok(exists);
        }

        // ✅ Needs token
        [Authorize]
        [HttpGet("CurrentUser")]
        public async Task<ActionResult> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                var error = Error.NotFound("User.NotFound", "No current user found");
                return HandleResult(Result<UserDTO>.Fail(error));
            }

            var result = await _authenticationService.GetUserByEmailAsync(email);
            return HandleResult(result);
        }

        // ✅ Admin only
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteUser")]
        public async Task<ActionResult> DeleteUser([FromQuery] string email)
        {
            var result = await _authenticationService.DeleteUserByEmailAsync(email);
            if (!result)
            {
                var error = Error.NotFound("User.NotFound", "No current user found");
                return HandleResult(Result<UserDTO>.Fail(error));
            }

            // لو HandleResult بيتعامل مع bool تمام:
            return HandleResult(result);
            // أو بدلها: return Ok("User deleted successfully");
        }

        // ✅ Public (Refresh عادةً بيكون بدون Authorization)
        [AllowAnonymous]
        [HttpPost("Refresh")]
        public async Task<ActionResult> Refresh([FromBody] RefreshRequestDTO dto)
        {
            var result = await _authenticationService.RefreshAsync(dto);
            return HandleResult(result);
        }

        // ✅ Needs token
        [Authorize]
        [HttpPost("Logout")]
        public async Task<ActionResult> Logout([FromBody] LogoutRequestDTO dto)
        {
            var result = await _authenticationService.LogoutAsync(dto);
            return HandleResult(result);
        }

        // ✅ Admin OR Owner (نفس المستخدم)
        [Authorize]
        [HttpPatch("UpdateUser/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            if (!User.IsInRole("Admin"))
            {
                var callerId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    User.FindFirstValue(JwtRegisteredClaimNames.Sub); // لو ضايف sub

                if (string.IsNullOrWhiteSpace(callerId) || callerId != id)
                    return Forbid();
            }

            var result = await _authenticationService.UpdateUserAsync(id, dto);
            return HandleResult(result);
        }

        // ✅ Admin OR Owner (نفس المستخدم)
        [Authorize]
        [HttpPatch("UpdatePassword/{id}")]
        public async Task<ActionResult> UpdatePassword(string id, [FromBody] UpdatePasswordDto dto)
        {
            if (!User.IsInRole("Admin"))
            {
                var callerId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    User.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (string.IsNullOrWhiteSpace(callerId) || callerId != id)
                    return Forbid();
            }

            var result = await _authenticationService.UpdatePassword(id, dto);
            return HandleResult(result);
        }
    }
}