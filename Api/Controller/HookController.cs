using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TelePush.Api.Service;

namespace TelePush.Api.Controller
{
    public class SetHookDAO
    {
        public string Hook { get; set; }
    }
    [Authorize]
    [Route("api/users/[controller]")]
    [ApiController]
    public class HookController : ControllerBase
    {

        private readonly UserService userService;
        private readonly HookService hookService;

        public HookController(UserService userService, HookService hookService)
        {
            this.userService = userService;
            this.hookService = hookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHook()
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var user = await userService.GetUserByIdAsync(id);

            return Ok(user.Hook);
        }

        [HttpPost]
        public async Task<IActionResult> SetHook(SetHookDAO hookDAO)
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (!await hookService.ValidateHook(hookDAO.Hook))
            {
                return BadRequest();
            }

            await userService.SetUserHookAsync(id, hookDAO.Hook);

            return Ok();
        }
    }
}