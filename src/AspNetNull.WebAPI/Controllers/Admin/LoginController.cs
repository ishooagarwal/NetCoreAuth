using AspNetNull.Persistance.Models.Application;
using AspNetNull.Persistance.Models.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNull.WebAPI.Controllers
{
    [AllowAnonymous]
    public class LoginController : ApiControllerBase
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<LoginController> logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration Configuration;

        public LoginController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginController> logger,
            IConfiguration Configuration)
            : base(logger)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = logger;
            this.Configuration = Configuration;
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginModel user, [FromQuery] string returnUrl = null)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogDebug($"{nameof(this.LoginAsync)} : Processing started.");

                    this.ValidationExecution<LoginModel>(user);

                    var nonSignedInUser = await this.userManager.FindByNameAsync(user.UserName);
                    var lockOutEndDate = await this.userManager.GetLockoutEndDateAsync(nonSignedInUser);
                    var lockoutEnabled = await this.userManager.GetLockoutEnabledAsync(nonSignedInUser);

                    if (lockoutEnabled && lockOutEndDate > DateTime.UtcNow && Convert.ToBoolean(this.Configuration["Env:EnableLockOut"]))
                    {
                        this.logger.LogDebug($"{nameof(this.LoginAsync)} : User is locked. Contact Admin.");
                        return Forbid("Account locked! Please contact your manager.");
                    }

                    if (userManager.SupportsUserLockout && userManager.GetAccessFailedCountAsync(nonSignedInUser).Result > Convert.ToInt32(this.Configuration["AccessFailedThreshold"]))
                    {
                        await this.userManager.SetLockoutEnabledAsync(nonSignedInUser, true);
                        await this.userManager.SetLockoutEndDateAsync(nonSignedInUser, DateTime.Now.AddDays(3));
                        return Unauthorized(new { Message = "Account Locked! Please contact your manager." });
                    }

                    if (this.userManager.CheckPasswordAsync(nonSignedInUser, user.Password).Result)
                    {
                        var result = await this.signInManager.PasswordSignInAsync(user.UserName, user.Password, user.RememberMe, false);

                        if (result.Succeeded)
                        {
                            this.logger.LogDebug($"{nameof(this.LoginAsync)} : Logged in user id : {nonSignedInUser.Id}");
                            var claims = await this.userManager.GetClaimsAsync(nonSignedInUser);

                            claims.Add(new System.Security.Claims.Claim("sub", nonSignedInUser.Id));

                            this.logger.LogDebug($"{nameof(this.LoginAsync)} : Logged in user offset : {nonSignedInUser.Offset.ToString()}");
                            claims.Add(new System.Security.Claims.Claim("offset", nonSignedInUser.Offset.ToString()));

                            var utf8 = new UTF8Encoding();
                            byte[] key = utf8.GetBytes("keyforsigninsecret");

                            var secretKey = new SymmetricSecurityKey(key);
                            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                            Stopwatch stopwatch = Stopwatch.StartNew();
                            DateTime expiresIn = DateTime.UtcNow.AddSeconds(Convert.ToDouble(this.Configuration["SessionExpiry"]));
                            

                            var jwtHeader = new JwtHeader(signinCredentials);
                            var jwtPayload = new JwtPayload(
                                issuer: "http://localhost:2000",
                                audience: "http://localhost:2000",
                                claims: claims,
                                expires: expiresIn,
                                notBefore: DateTime.UtcNow
                                );

                            var tokenOptions = new JwtSecurityToken(jwtHeader, jwtPayload);

                            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                            await this.userManager.AddLoginAsync(nonSignedInUser,
                                new UserLoginInfo(this.Configuration["provider"], this.Configuration["providerId"], this.Configuration["providerDisplayName"]));
                            expiresIn = expiresIn.AddMilliseconds(stopwatch.ElapsedMilliseconds * -1);
                            stopwatch.Stop();
                            return Ok(new { Token = tokenString, ExpiresIn = expiresIn });
                        }
                        else
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        this.logger.LogDebug($"{nameof(this.LoginAsync)} : Invalid attempt by user : {nonSignedInUser.Id}");
                        if (this.userManager.SupportsUserLockout && this.userManager.GetLockoutEnabledAsync(nonSignedInUser).Result)
                        {
                            await this.userManager.AccessFailedAsync(nonSignedInUser);
                        }
                        return Unauthorized();
                    }
                }, nameof(this.LoginAsync));
        }
    }
}
