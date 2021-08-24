using AutoMapper;
using AspNetNull.Persistance.Helpers;
using AspNetNull.Persistance.Models.Application;
using AspNetNull.Persistance.Models.Email;
using AspNetNull.Persistance.Models.ErrorResponses;
using AspNetNull.Persistance.Models.Password;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetNull.WebAPI.Controllers.Admin
{
    public class ResetPasswordController : ApiControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<LoginController> logger;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;

        public ResetPasswordController(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<LoginController> logger,
            IConfiguration configuration)
            : base(logger, userManager, mapper)
        {
            this.userManager = userManager;
            this.logger = logger;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        [HttpPost, Route("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogInformation($"{nameof(this.ForgotPassword)} : Processing started for {model.Email.MaskEmail()}.");

                    this.ValidationExecution<ForgotPasswordRequest>(model);

                    var user = await this.userManager.FindByEmailAsync(model.Email);
                    if (user == null || !(await this.userManager.IsEmailConfirmedAsync(user)))
                    {
                        this.logger.LogDebug($"{nameof(this.ForgotPassword)} : user does not exist.");
                        // Don't reveal that the user does not exist or is not confirmed
                        return BadRequest();
                    }

                    var code = await this.userManager.GeneratePasswordResetTokenAsync(user);

                    string finalCode = ProjectHelpers.MergeUserIdWithCode(code, user.Id);

                    var message = new Message()
                    {
                        Subject = $"{configuration["Organization"]} - Password Reset Request.",
                        Body = $"Click here {configuration["ResetPasswordURL"]}{finalCode}",
                        Destination = model.Email,
                        SenderEmail = this.configuration["SenderEmail"]
                    };

                    this.logger.LogInformation($"{nameof(this.ForgotPassword)} : Sending mail to {message.Destination.MaskEmail()}, from {message.SenderEmail}");

                    ProjectHelpers.SendMail(message, this.configuration["SecretKet"]);

                    return Ok();
                }, nameof(this.ForgotPassword));
        }

        [HttpPost, Route("passwordReset")]
        public async Task<IActionResult> PasswordReset([FromBody] ResetPasswordRequest model, [FromQuery] string code)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogInformation($"{nameof(this.ForgotPassword)} : Processing started.");

                    this.ValidationExecution<ResetPasswordRequest>(model);

                    if (string.IsNullOrEmpty(code))
                    {
                        throw new ValidationException("Invalid request.");
                    }
                    string userId = string.Empty;
                    string token = ProjectHelpers.SplitUserIdWithCode(code, out userId, 36);

                    var user = await this.userManager.FindByIdAsync(userId);

                    if (user == null)
                    {
                        BadRequest();
                    }

                    var identityResult = await this.userManager.ResetPasswordAsync(user, token, model.NewPassword);

                    return await this.IdentityResultResponse<ResetPasswordResponse>(identityResult, "password reset successfully", HttpStatusCode.Unauthorized);
                }, nameof(this.PasswordReset));
        }

        [HttpPost, Route("changepassword/current")]
        [Authorize(Constants.Policies.Employee)]
        public async Task<IActionResult> ChangePassword([FromBody] ResetPasswordRequest model)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogInformation($"{nameof(this.ChangePassword)} : Processing started.");

                    this.ValidationExecution<ResetPasswordRequest>(model);

                    var user = await this.GetApplicationUser();

                    if (user == null)
                    {
                        BadRequest();
                    }

                    var identityResult = await this.userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                    return await this.IdentityResultResponse<ResetPasswordResponse>(identityResult, "password reset successfully", HttpStatusCode.Unauthorized);
                }, nameof(this.ChangePassword));
        }
    }
}
