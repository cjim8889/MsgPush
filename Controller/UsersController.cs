﻿using System;
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
    [Route("api/[controller]")]
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<List<User>> GetUsersAsync()
        {
            return await userService.GetUsersAsync();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> CreateUser(UserDTO userDTO)
        {
            var isAdmin = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role && c.Value == Role.Admin) != null;

            if (!isAdmin) {
                if (string.IsNullOrWhiteSpace(userDTO.RecaptchaToken))
                {
                    return BadRequest(new ReturnMessage() { StatusCode = Model.StatusCode.EmptyRecaptchaToken, Message = ResponseMessage.EmptyRecaptchaToken});
                }

                if (!await recaptchaService.Authenticate(userDTO.RecaptchaToken))
                {
                    return BadRequest(new ReturnMessage() { StatusCode = Model.StatusCode.InvalidRecaptchaToken, Message = ResponseMessage.InvalidRecaptchaToken });
                }
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

        //[Authorize(Roles = "Admin")]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user != null)
            {
                return Ok(user);
            }

            return NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/roles")]
        public async Task<ActionResult> GetUserRolesById(string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user != null)
            {
                return Ok(user.Roles);
            }

            return NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/roles/{role}")]
        public async Task<ActionResult> AddRoleToUser(string id, string role)
        {
            var result = await userService.AddRoleToUserAsync(id, role);

            return result ? Ok() : (ActionResult)BadRequest();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/roles")]
        public async Task<ActionResult> AddRolesToUser(string id, [FromBody] AddRoles addRolesPost)
        {
            var result = await userService.AddRolesToUserAsync(id, addRolesPost.Roles);

            return result ? Ok() : (ActionResult)BadRequest();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}/roles/{role}")]
        public async Task<ActionResult> RemoveRoleOfUser(string id, string role)
        {
            var result = await userService.RemoveRoleOfUserAsync(id, role);

            return result ? Ok() : (ActionResult)BadRequest();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}/roles")]
        public async Task<ActionResult> RemoveRoleOfUser(string id, [FromBody] RemoveRoles roles)
        {
            var result = await userService.RemoveRolesOfUserAsync(id, roles.Roles);

            return result ? Ok() : (ActionResult)BadRequest();
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveUserById(string id)
        {
            await userService.RemoveUserByIdAsync(id);
            return NoContent();
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