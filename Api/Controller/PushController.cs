using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TelePush.Api.Service;
using TelePush.Api.Model;
using Microsoft.AspNetCore.Authorization;


namespace TelePush.Api.Controller
{
    [ApiVersion("1.0")]
    [AllowAnonymous]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiController]
    public class PushController : ControllerBase
    {
        private readonly UserService userService;
        private readonly MqService mqService;
        public PushController(UserService userService, MqService mqService)
        {
            this.userService = userService;
            this.mqService = mqService;
        }

        [HttpGet]
        public async Task<ActionResult> PushMessage([FromQuery(Name = "t")] string pushToken, [FromQuery(Name = "m")] string message)
        {
            var user = await userService.GetUserByPushToken(pushToken);
            if (user == null)
            {
                return BadRequest();
            }

            await mqService.PushMessage(message, user.Subscribers, user.Hook);
            return Ok(new ReturnMessage() { StatusCode=Model.StatusCode.Success });
        }
    }
}