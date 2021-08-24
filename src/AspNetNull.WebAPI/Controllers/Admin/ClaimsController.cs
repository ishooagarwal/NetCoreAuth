using AutoMapper;
using AspNetNull.Persistance.Helpers;
using AspNetNull.Persistance.Models.Application;
using AspNetNull.Persistance.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PolicyClaimRequest = AspNetNull.Persistance.Models.Claims.Requests;
using PolicyClaimResponse = AspNetNull.Persistance.Models.Claims.Responses;

namespace AspNetNull.WebAPI.Controllers.Admin
{
    [Authorize(Policy = Constants.Policies.Administrator)]
    public class ClaimsController : ApiControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<ClaimsController> logger;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly IUnitOfWork unitOfWork;

        public ClaimsController(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<ClaimsController> logger,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
            : base(logger, userManager, mapper)
        {
            this.userManager = userManager;
            this.logger = logger;
            this.mapper = mapper;
            this.configuration = configuration;
            this.unitOfWork = unitOfWork;
        }


        [HttpGet, Route("claims/{id:length(36)}")]
        public async Task<IActionResult> RetrieveClaims(string id)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogInformation($"RetrieveClaims started for id {id}");

                    var claim = await this.unitOfWork.ClaimRepository.GetFirstOrDefaultAsync(x => x.Id.Equals(id) && x.IsDisabled == false);

                    var claimResponse = this.mapper.Map<PolicyClaimResponse.ClaimResponse>(claim);
                    
                    this.AddOffsetToTimeFrames(claimResponse, await this.GetApplicationUser());

                    return Ok(claim);
                }, nameof(this.RetrieveClaims));
        }

        [HttpGet, Route("claims")]
        public async Task<IActionResult> RetrieveClaims()
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogInformation($"RetrieveClaims started for all claims.");

                    var claims = await this.unitOfWork.ClaimRepository.GetAllAsync();

                    var claimResponse = this.mapper.Map<IEnumerable<PolicyClaimResponse.ClaimResponse>>(claims);

                    foreach (var claim in claimResponse)
                    {
                        this.AddOffsetToTimeFrames(claim, await this.GetApplicationUser());
                    }

                    return Ok(claimResponse);
                }, nameof(this.RetrieveClaims));
        }

        [HttpPost, Route("claims")]
        public async Task<IActionResult> CreateClaims([FromBody] IList<PolicyClaimRequest.ClaimRequest> claim)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogInformation($"CreateClaims : started for {claim.Count} claims.");

                    var user = await this.GetApplicationUser(); ;

                    foreach (var clm in claim)
                    {
                        clm.Id = ProjectHelpers.GenerateUniqueId(null);

                        this.logger.LogInformation($"CreateClaims : started for id {clm.Id}");
                        this.logger.LogInformation($"{nameof(this.CreateClaims)} : Claim name {clm.Name}");

                        clm.IsDisabled = false;
                        clm.ConcurrencyStamp = ProjectHelpers.GenerateUniqueId(null);
                        clm.CreatedAt = DateTime.UtcNow;
                        clm.CreatedBy = user.Id;

                        this.ValidationExecution<PolicyClaimRequest.ClaimRequest>(clm);
                    }

                    await this.unitOfWork.ClaimRepository.AddRangeAsync(claim.ToArray());
                    this.unitOfWork.Save();

                    return Ok();
                }, nameof(this.CreateClaims));
        }

        [HttpPut, Route("claims")]
        public async Task<IActionResult> UpdateClaims([FromBody] IList<PolicyClaimRequest.ClaimRequest> claim)
        {
            return await this.ExecuteWithErrorHandlingAsync(async () => await this.ModifyClaims(claim), nameof(this.UpdateClaims));
        }

        [HttpDelete, Route("claims")]
        public async Task<IActionResult> DeleteClaims([FromBody] IList<PolicyClaimRequest.ClaimRequest> claim)
        {
            return await this.ExecuteWithErrorHandlingAsync(async () => await this.ModifyClaims(claim, true), nameof(this.DeleteClaims));
        }

        private async Task<IActionResult> ModifyClaims(IList<PolicyClaimRequest.ClaimRequest> claim, bool isDisabled = false)
        {
            this.logger.LogInformation($"ModifyClaims : started for isDisabled {isDisabled}");

            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var PolicyClaim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            var user = await this.userManager.FindByIdAsync(PolicyClaim.Value);

            foreach (var clm in claim)
            {
                this.logger.LogInformation($"ModifyClaims : started for id {clm.Id}");
                this.logger.LogInformation($"{nameof(this.ModifyClaims)} : Claim name {clm.Name}");

                clm.IsDisabled = isDisabled;
                clm.UpdatedAt = DateTime.UtcNow;
                clm.UpdatedBy = user.Id;

                this.ValidationExecution<PolicyClaimRequest.ClaimRequest>(clm);
            }

            await this.unitOfWork.ClaimRepository.UpdateRangeAsync(claim.ToArray());
            this.unitOfWork.Save();

            return Ok();
        }
    }
}
