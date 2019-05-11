using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MsgPush.Service;
using MsgPush.Model;
using Microsoft.AspNetCore.Authorization;


namespace MsgPush.Controller
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/users")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly UserService userService;
        public LoginController(UserService userService)
        {
            this.userService = userService;
        }


        [AllowAnonymous]
        [HttpGet("login/{adminToken}")]
        public async Task<ActionResult> LogInByAdminToken(string adminToken)
        {
            var user = await userService.GetUserByAdminTokenAsync(adminToken);

            return user != null
                ? Ok(new ReturnMessage() { StatusCode = Model.StatusCode.Success, Message = userService.GenerateJwtToken(user, DateTime.Now.AddHours(1))})
                : (ActionResult)BadRequest(new ReturnMessage() { StatusCode = Model.StatusCode.InvalidAdminToken, Message = ResponseMessage.InvalidAdminToken});
        }

        [AllowAnonymous]
        [HttpGet("login")]
        public async Task<ActionResult> LogIn([FromQuery] string username, [FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new ReturnMessage() { StatusCode = Model.StatusCode.InvalidUsernameOrPassword, Message = ResponseMessage.EmptyUsernameOrPassword});
            }

            var user = await userService.GetUserByUsernameAndPasswordAsync(username, password);

            return user != null
                ? Ok(new ReturnMessage() { StatusCode = Model.StatusCode.Success, Message = userService.GenerateJwtToken(user, DateTime.Now.AddHours(1)) })
                : (ActionResult)BadRequest(new ReturnMessage() { StatusCode = Model.StatusCode.InvalidUsernameOrPassword, Message = ResponseMessage.InvalidUsernameOrPassword });
        }
    }
}