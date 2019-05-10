using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using MsgPush.Service;
using MsgPush.Model;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;

namespace MsgPush.Controller
{
    [Route("api/validate")]
    [ApiController]
    public class ValidationController : ControllerBase
    {
        private readonly UserService userService;
        private readonly IAuthService authService;
        public ValidationController(UserService userService, IAuthService authService)
        {
            this.userService = userService;
            this.authService = authService;
        }


        [Authorize]
        [HttpGet("new")]
        public ActionResult ValidationRequest([FromQuery]long receiverId)
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            authService.New(id);
            
            return Ok(new ReturnMessage() { StatusCode = Model.StatusCode.Success });
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Validate(string challengeCode)
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (!authService.Authenticate(id, challengeCode))
            {
                return BadRequest(new ReturnMessage() { StatusCode = Model.StatusCode.InvalidRecaptchaToken });
            }
        
            await userService.SetUserValidationAsync(id, true);

            return Ok();
        }

    }

}