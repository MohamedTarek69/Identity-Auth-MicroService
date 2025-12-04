using Identity_Auth_MicroService.Shared.CommonResult;
using Identity_Auth_MicroService.Shared.IdentityDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity_Auth_MicroService.Services_Abstraction.Interfaces
{
    public interface IAuthenticationService
    {
        // Login
        // Email, Password => Token, Display Name, Email
        Task<Result<LoginReturnedDataDTO>> LoginAsync(LoginDTO loginDTO);
        // Register
        // Display Name, PhoneNumber, Email, Password => Token, Display Name, Email
        Task<Result<UserDTO>> RegisterAsync(RegisterDTO registerDTO);
        Task<bool> CheckEmailAsync(string Email);
        Task<Result<LoginReturnedDataDTO>> GetUserByEmailAsync(string Email);
    }
}
