using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MsgPush.Service;
using MsgPush.Model;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace MsgPush.Controller
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

            await mqService.PushMessage(message, user.Subscribers);
            return Ok(new ReturnMessage() { StatusCode=Model.StatusCode.Success });
        }


    }
}