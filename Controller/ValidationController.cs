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
        private readonly MqService mqService;
        public ValidationController(UserService userService, IAuthService authService, MqService mqService)
        {
            this.userService = userService;
            this.authService = authService;
            this.mqService = mqService;
        }


        [Authorize]
        [HttpGet("new")]
        public async Task<ActionResult> ValidationRequest([FromQuery]long receiverId)
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var code = authService.New(id, receiverId);
            await mqService.PushChallengeMessage(code, receiverId);
            
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
        
            // await userService.SetUserValidationAsync(id, true);
            await userService.AddSubsriberToUserAsync(id, authService.GetSubsriberId(id));
            authService.RemoveKey(id);

            return Ok();
        }
    }

}