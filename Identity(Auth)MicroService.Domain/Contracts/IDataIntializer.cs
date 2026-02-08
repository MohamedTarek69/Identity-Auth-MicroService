using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity_Auth_MicroService.Domain.Contracts
{
    public interface IDataIntializer
    {
        Task IntializeAsync();
    }
}
