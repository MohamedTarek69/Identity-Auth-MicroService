using Identity_Auth_MicroService.Shared.CommonResult;
using Identity_Auth_MicroService.Shared.IdentityDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
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
        // Login
        // Post: BaseUrl/api/authentication/login
        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var result = await _authenticationService.LoginAsync(loginDTO);
            return HandleResult(result);
        }

        // Register
        // Post: BaseUrl/api/authentication/register
        [HttpPost("Register")]
        public async Task<ActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var result = await _authenticationService.RegisterAsync(registerDTO);
            return HandleResult(result);
        }

        [HttpGet("EmailExists")]
        public async Task<ActionResult<bool>> CheckEmail(string email)
        {
            var exists = await _authenticationService.CheckEmailAsync(email);
            return Ok(exists);
        }

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

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteUser")]
        public async Task<ActionResult> DeleteUser(string email)
        {
            var result = await _authenticationService.DeleteUserByEmailAsync(email);
            if (!result)
            {
                var errors = Error.NotFound("User.NotFound", "No current user found");
                return HandleResult(Result<UserDTO>.Fail(errors));
            }
            return Ok("User deleted successfully");
        }

        [HttpPost("Refresh")]
        public async Task<ActionResult> Refresh([FromBody] RefreshRequestDTO dto)
        {
            var result = await _authenticationService.RefreshAsync(dto);
            return HandleResult(result);
        }

        [HttpPost("Logout")]
        public async Task<ActionResult> Logout([FromBody] LogoutRequestDTO dto)
        {
            var result = await _authenticationService.LogoutAsync(dto);
            return HandleResult(result);
        }


    }
}
