using AutoMapper;
using AspNetNull.Persistance.Models.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetNull.WebAPI.Controllers.Admin
{
    [Authorize(Policy = Constants.Policies.Guest)]
    public class LogoutController : ApiControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<LogoutController> logger;
        private readonly IConfiguration Configuration;

        public LogoutController(
            UserManager<ApplicationUser> userManager,
            ILogger<LogoutController> logger,
            IConfiguration Configuration)
            : base(logger, userManager)
        {
            this.userManager = userManager;
            this.logger = logger;
            this.Configuration = Configuration;
        }

        [HttpGet, Route("logout/invalidatetoken")]
        public async Task<IActionResult> InvalidateToken([FromQuery]string token)
        { 
            return await this.ExecuteWithErrorHandlingAsync(
                async () => {
                    this.logger.LogInformation($"{nameof(this.InvalidateToken)} : Processing started.");

                    var user = await this.GetApplicationUser();

                    var identityResult = await this.userManager.RemoveAuthenticationTokenAsync(user, this.Configuration["providerDisplayName"], token);

                    return Ok();
                });
        }
    }
}
