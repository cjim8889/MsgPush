using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TelePush.Api.Service;

namespace Api.Controller
{
    [Authorize]
    [Route("api/users/[controller]")]
    [ApiController]
    public class HookController : ControllerBase
    {

        private readonly UserService userService;
        public HookController(UserService userService)
        {
            this.userService = userService;
        }

        public async Task<IActionResult> GetHook()
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var user = await userService.GetUserByIdAsync(id);

            return Ok(user.Hook);
        }
    }
}