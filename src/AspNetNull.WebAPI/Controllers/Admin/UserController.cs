using AutoMapper;
using AspNetNull.Persistance.Helpers;
using AspNetNull.Persistance.Models.Application;
using AspNetNull.Persistance.Models.ErrorResponses;
using AspNetNull.Persistance.Models.User;
using AspNetNull.Persistance.Models.UserClaims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetNull.WebAPI.Controllers
{
    [Authorize(Policy = Constants.Policies.Administrator)]
    public class UserController : ApiControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<LoginController> logger;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IMapper mapper;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMapper mapper,
            ILogger<LoginController> logger)
            : base(logger, userManager, mapper)
        {
            this.userManager = userManager;
            this.logger = logger;
            this.roleManager = roleManager;
            this.mapper = mapper;
        }

        [HttpPost, Route("user")]
        public async Task<IActionResult> CreateUserAsync([FromBody] ApplicationUser user)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogInformation($"{nameof(this.CreateUserAsync)} : Processing started for {user?.Email?.MaskEmail()}.");

                    this.ValidationExecution<ApplicationUser>(user);
                    IdentityResult result = await userManager.CreateAsync(user, user.PasswordHash);

                    return await this.IdentityResultResponse<CreateUserResponse>(result, "User created successfully");
                }, nameof(this.CreateUserAsync));
        }

        [HttpPost, Route("user/current/claims")]
        public async Task<IActionResult> MapClaimsToUserAsync([FromBody] IList<Claims> claims)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogInformation($"{nameof(this.MapClaimsToUserAsync)} : Processing started for claims : {claims?.Count}.");

                    this.ValidationExecution<IList<Claims>>(claims);

                    var user = await this.GetApplicationUser();

                    if (user != null)
                    {
                        var identityResult = await this.userManager.AddClaimsAsync(user, this.ConvertClaims(claims));

                        return await this.IdentityResultResponse<CreateUserResponse>(identityResult, "Claims mapped to user");
                    }
                    else
                    {
                        return BadRequest();
                    }
                }, nameof(this.MapClaimsToUserAsync));
        }

        [HttpPost, Route("user/{userId:length(36)}/claims")]
        public async Task<IActionResult> MapClaimsToUserAsync([FromBody] IList<Claims> claims, string userId)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogInformation($"{nameof(this.MapClaimsToUserAsync)} : Processing started for claims : {claims?.Count} and userId {userId}.");

                    this.ValidationExecution<IList<Claims>>(claims);

                    var user = await this.userManager.FindByIdAsync(userId);

                    if (user != null)
                    {
                        var identityResult = await this.userManager.AddClaimsAsync(user, this.ConvertClaims(claims));

                        return await this.IdentityResultResponse<CreateUserResponse>(identityResult, "Claim mapped to user");
                    }
                    else
                    {
                        return BadRequest();
                    }
                }, nameof(this.MapClaimsToUserAsync));
        }

        [HttpGet, Route("user/current/claims")]
        public async Task<IActionResult> GetUserClaims()
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogInformation($"{nameof(this.GetUserClaims)} : Processing started.");

                    var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                    var securityClaims = claimsIdentity.Claims;

                    var claimsResponse = this.mapper.Map<IEnumerable<Claims>>(securityClaims);

                    return ProjectHelpers.GenerateResult(claimsResponse, System.Net.HttpStatusCode.OK);
                }, nameof(this.GetUserClaims));
        }

        [HttpGet, Route("users")]
        public async Task<IActionResult> GetUser()
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogInformation($"{nameof(this.GetUser)} : Processing started.");

                    var applicationUsers = this.userManager.Users.ToList();

                    var users = this.mapper.Map<IEnumerable<RetrieveUser>>(applicationUsers);
                    return ProjectHelpers.GenerateResult(users, System.Net.HttpStatusCode.OK);
                }, nameof(this.GetUser));
        }

        [HttpGet, Route("users/current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogInformation($"{nameof(this.GetCurrentUser)} : Processing started.");

                    var applicationUser = await this.GetApplicationUser();

                    var user = this.mapper.Map<RetrieveUser>(applicationUser);
                    return ProjectHelpers.GenerateResult(user, System.Net.HttpStatusCode.OK);
                }, nameof(this.GetCurrentUser));
        }

        private IEnumerable<Claim> ConvertClaims(IList<Claims> claims)
        {
            IList<Claim> securityClaims = new List<Claim>();
            foreach (Claims clm in claims)
            {
                securityClaims.Add(new Claim(clm.Name, clm.Value));
            }
            return securityClaims;
        }
    }
}
