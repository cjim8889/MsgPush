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
    [Route("v{version:apiVersion}/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public class UserDTO
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string RecaptchaToken {get; set;}
        }

        public class AddRoles
        {
            public string[] Roles { get; set; }
        }

        public class RemoveRoles
        {
            public string[] Roles { get; set; }
        }

        private readonly UserService userService;
        private readonly RecaptchaService recaptchaService;
        public UsersController(UserService userService, RecaptchaService recaptchaService)
        {
            this.userService = userService;
            this.recaptchaService = recaptchaService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> CreateUser(UserDTO userDTO)
        {

            if (string.IsNullOrWhiteSpace(userDTO.RecaptchaToken))
            {
                return BadRequest(new ReturnMessage() { StatusCode = Model.StatusCode.EmptyRecaptchaToken, Message = ResponseMessage.EmptyRecaptchaToken});
            }

            if (!await recaptchaService.Authenticate(userDTO.RecaptchaToken))
            {
                return BadRequest(new ReturnMessage() { StatusCode = Model.StatusCode.InvalidRecaptchaToken, Message = ResponseMessage.InvalidRecaptchaToken });
            }
            
            var user = new User() { Username = userDTO.Username, Password = userDTO.Password };

            if (!userService.ValidateUserData(user))
            {
                return BadRequest(new ReturnMessage() { StatusCode = Model.StatusCode.InvalidUsernameOrPassword, Message = ResponseMessage.InvalidUsernameOrPassword });
            }

            if (await userService.IsUsernameExistsAsync(user.Username))
            {
                return BadRequest(new ReturnMessage() { StatusCode = Model.StatusCode.DuplicateUsername, Message = ResponseMessage.DuplicateUsername });
            }


            await userService.CreateUserAsync(user);
            return CreatedAtAction("CreateUser", user);
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<ActionResult<User>> GetUserByClaim()
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            return Ok(await userService.GetUserByIdAsync(id));
        }

        [Authorize]
        [HttpGet("user/token/refresh")]
        public async Task<ActionResult> RefreshUserToken()
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var tokens = await userService.RefreshUserToken(id);

            return Ok(new { tokens.AdminToken, tokens.PushToken });
        }

        [Authorize(Roles = "Standard")]
        [HttpDelete("user")]
        public async Task<ActionResult> RemoveUserByUser()
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (!string.IsNullOrWhiteSpace(id))
            {
                await userService.RemoveUserByIdAsync(id);
                return NoContent();
            }

            return BadRequest();
        }

        [Authorize]
        [HttpGet("user/password/change")]
        public async Task<ActionResult> ChangeUserPassword(string password)
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (await userService.ChangeUserPassword(id, password))
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}