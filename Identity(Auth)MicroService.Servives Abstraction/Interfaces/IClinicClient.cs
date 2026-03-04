using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity_Auth_MicroService.Servives_Abstraction.Interfaces
{
    public interface IClinicClient
    {
        Task<bool> IsDoctorActiveAsync(string identityUserId);
    }
}
