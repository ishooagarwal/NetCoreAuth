using AutoMapper;
using AspNetNull.Persistance.Helpers;
using AspNetNull.Persistance.Models.Application;
using AspNetNull.Persistance.Models.Email;
using AspNetNull.Persistance.Models.ErrorResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetNull.WebAPI.Controllers.Admin
{
    [Authorize(Policy = Constants.Policies.Employee)]
    public class EmailController : ApiControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<EmailController> logger;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        public EmailController(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<EmailController> logger,
            IConfiguration configuration)
            : base(logger, userManager, mapper)
        {
            this.userManager = userManager;
            this.logger = logger;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        [HttpPost, Route("verifyemail/current")]
        public async Task<IActionResult> VerifyEmail()
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogDebug($"{nameof(this.VerifyEmail)} : Procesisng started.");

                    var applicationUser = await this.GetApplicationUser();

                    if (applicationUser == null)
                    {
                        this.logger.LogDebug($"{nameof(this.VerifyEmail)} : Application user not found.");
                        return BadRequest();
                    }

                    var token = await this.userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

                    string confirmationToken = ProjectHelpers.MergeUserIdWithCode(token, applicationUser.Id);

                    var message = new Message()
                    {
                        Subject = $"{configuration["Organization"]} - Password Reset Request.",
                        Body = $"Click here {configuration["ResetPasswordURL"]}{confirmationToken}",
                        Destination = applicationUser.Email,
                        SenderEmail = this.configuration["SenderEmail"]
                    };

                    this.logger.LogInformation($"{nameof(this.VerifyEmail)} : Sending mail to {message.Destination.MaskEmail()}, from {message.SenderEmail}");

                    ProjectHelpers.SendMail(message, this.configuration["SecretKey"]);

                    return Ok();
                }, nameof(this.VerifyEmail));
        }

        [HttpGet, Route("verifyemail")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string c)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogDebug($"{nameof(this.VerifyEmail)} : Confirm email procesisng started.");
                    string userId = string.Empty;
                    string confirmationToken = ProjectHelpers.SplitUserIdWithCode(c, out userId, Constants.Miscllaneous.ConfirmationTokenIntruderLength);

                    var user = await this.userManager.FindByIdAsync(userId);

                    if (user == null)
                    {
                        this.logger.LogDebug($"{nameof(this.VerifyEmail)} : Application user not found.");
                        BadRequest();
                    }

                    var identityResult = await this.userManager.ConfirmEmailAsync(user, confirmationToken);

                    return await this.IdentityResultResponse<EmailVerificationResponse>(identityResult, "Email verified");
                }, nameof(this.VerifyEmail));
        }
    }
}
