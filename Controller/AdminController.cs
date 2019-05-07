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
    // [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserService userService;
        public AdminController(UserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("users")]
        public async Task<List<User>> GetUsersAsync()
        {
            return await userService.GetUsersAsync();
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user != null)
            {
                return Ok(user);
            }

            return NotFound();
        }

        [HttpGet("users/{id}/roles")]
        public async Task<ActionResult> GetUserRolesById(string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user != null)
            {
                return Ok(user.Roles);
            }

            return NotFound();
        }

        [HttpPut("users/{id}/roles/{role}")]
        public async Task<ActionResult> AddRoleToUser(string id, string role)
        {
            var result = await userService.AddRoleToUserAsync(id, role);

            return result ? Ok() : (ActionResult)BadRequest();
        }

        [HttpDelete("users/{id}/roles/{role}")]
        public async Task<ActionResult> RemoveRoleOfUser(string id, string role)
        {
            var result = await userService.RemoveRoleOfUserAsync(id, role);

            return result ? Ok() : (ActionResult)BadRequest();
        }

        [HttpDelete("users/{id}")]
        public async Task<ActionResult> RemoveUserById(string id)
        {
            await userService.RemoveUserByIdAsync(id);
            return NoContent();
        }

        [HttpPost("users")]
        public async Task<ActionResult> CreateUser(UsersController.UserDTO userDTO)
        {

            var user = new User() { Username = userDTO.Username, Password = userDTO.Password };
            await userService.CreateUserAsync(user);


            return CreatedAtAction("CreateUser", user);
        }


    }
}