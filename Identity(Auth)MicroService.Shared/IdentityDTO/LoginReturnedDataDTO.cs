using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity_Auth_MicroService.Shared.IdentityDTO
{
    public record LoginReturnedDataDTO
    (
        string DisplayName,
        string Email,
        string Token
    );
}
