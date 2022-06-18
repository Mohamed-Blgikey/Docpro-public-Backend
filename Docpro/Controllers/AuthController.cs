using Docpro.BL.Dtos;
using Docpro.BL.Helper;
using Docpro.BL.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Docpro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        #region fields
        private readonly IAuthservice authservice;
        private readonly IHubContext<NotifyHub> hub;
        #endregion

        #region ctor
        public AuthController(IAuthservice authservice, IHubContext<NotifyHub> hub)
        {
            this.authservice = authservice;
            this.hub = hub;
        } 
        #endregion

        #region Login
        [HttpPost]
        [Route("~/Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await authservice.Login(loginDTO);
            if (!result.IsAuthencated)
            {
                return Ok(new { message = result.Message });
            }

            return Ok(new { message = result.Message, token = result.Token, expiresOn = result.ExpiresOn });
        }
        #endregion
     
        #region Register
        [HttpPost]
        [Route("~/Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await authservice.Register(registerDTO);
            if (!result.IsAuthencated)
                return Ok(new { message = result.Message });

            await  hub.Clients.All.SendAsync("NewUser");
            return Ok(new { message = result.Message, token = result.Token, expiresOn = result.ExpiresOn });
        }
        #endregion

    }
}
