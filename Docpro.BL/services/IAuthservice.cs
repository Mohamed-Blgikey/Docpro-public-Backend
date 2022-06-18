
using Docpro.BL.Dtos;
using Docpro.BL.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.services
{
    public interface IAuthservice
    {
        Task<AuthModel> Register(RegisterDTO registerDTO);
        Task<AuthModel> Login(LoginDTO loginDTO);

    }
}
