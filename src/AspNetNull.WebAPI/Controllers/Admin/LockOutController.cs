using AutoMapper;
using AspNetNull.Persistance.Helpers;
using AspNetNull.Persistance.Models.Application;
using AspNetNull.Persistance.Models.LockOut;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AspNetNull.WebAPI.Controllers.Admin
{
    [Authorize(Policy = Constants.Policies.Guest)]
    public class LockOutController : ApiControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<LockOutController> logger;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        public LockOutController(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<LockOutController> logger,
            IConfiguration configuration)
            : base(logger, userManager, mapper)
        {
            this.userManager = userManager;
            this.logger = logger;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        [HttpPost, Route("user/lock")]
        public async Task<IActionResult> LockUser([FromBody] LockUserRequest lockUserRequest)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                   async () =>
                   {
                       this.logger.LogDebug($"{nameof(this.LockUser)} : processing started.");
                       if (!this.userManager.SupportsUserLockout)
                       {
                           this.logger.LogDebug($"{nameof(this.LockUser)} : feature does not support lockout.");

                           return new LockOutResponse()
                           {
                               HttpStatusCode = System.Net.HttpStatusCode.BadRequest,
                               Message = $"Does not support user lockout."
                           }
                           .ObjectResultFromStatus();
                       }

                       this.logger.LogInformation($"{nameof(this.LockUser)} : initiating lockuse for userid : {lockUserRequest.UserId}.");
                       
                       var nonSignedInUser = await this.userManager.FindByIdAsync(lockUserRequest.UserId);
                       var lockOutEndDate = await this.userManager.GetLockoutEndDateAsync(nonSignedInUser);

                       if (lockOutEndDate.HasValue)
                       {
                           TimeSpan ts = new TimeSpan(
                               lockOutEndDate.Value.Day,
                               lockOutEndDate.Value.Hour,
                               lockOutEndDate.Value.Minute,
                               lockOutEndDate.Value.Second,
                               lockOutEndDate.Value.Millisecond);
                           lockUserRequest.LockOutEndDate.Add(ts);
                       }

                       await this.userManager.SetLockoutEnabledAsync(nonSignedInUser, true);
                       await this.userManager.SetLockoutEndDateAsync(nonSignedInUser, lockUserRequest.LockOutEndDate);
                       
                       this.logger.LogDebug($"{nameof(this.LockUser)} : user is locked till : {lockOutEndDate.Value}.");
                       
                       return new LockOutResponse()
                       {
                           HttpStatusCode = System.Net.HttpStatusCode.OK,
                           Message = $"User locked till {lockUserRequest.LockOutEndDate.ToString()}."
                       }
                       .ObjectResultFromStatus();
                   }, nameof(this.LockUser));
        }

        [HttpPost, Route("user/unlock")]
        public async Task<IActionResult> UnlockUser([FromBody] string userId)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                   async () =>
                   {
                       this.logger.LogInformation($"{nameof(this.UnlockUser)} : Procesisng started..");

                       if (!this.userManager.SupportsUserLockout)
                       {
                           this.logger.LogInformation($"{nameof(this.LockUser)} : feature does not support lockout.");

                           return new LockOutResponse()
                           {
                               HttpStatusCode = System.Net.HttpStatusCode.BadRequest,
                               Message = $"Does not support user lockout."
                           }
                           .ObjectResultFromStatus();
                       }
                       this.logger.LogInformation($"{nameof(this.LockUser)} : initiating unlock for userid : {userId}.");

                       var nonSignedInUser = await this.userManager.FindByIdAsync(userId);

                       await this.userManager.SetLockoutEnabledAsync(nonSignedInUser, false);
                       await this.userManager.SetLockoutEndDateAsync(nonSignedInUser, null);
                       await this.userManager.ResetAccessFailedCountAsync(nonSignedInUser);

                       this.logger.LogInformation($"{nameof(this.LockUser)} : user is unlocked.");

                       return new LockOutResponse()
                       {
                           HttpStatusCode = System.Net.HttpStatusCode.OK,
                           Message = $"User unlocked."
                       }
                       .ObjectResultFromStatus();
                   }, nameof(this.UnlockUser));
        }
    }
}
