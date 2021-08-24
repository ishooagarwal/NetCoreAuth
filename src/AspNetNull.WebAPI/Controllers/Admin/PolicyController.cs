using AutoMapper;
using AspNetNull.Persistance.Helpers;
using AspNetNull.Persistance.Models.Application;
using AspNetNull.Persistance.Models.Policy;
using AspNetNull.Persistance.Models.Policy.Requests;
using AspNetNull.Persistance.Models.UserClaims;
using AspNetNull.Persistance.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolicyClaimsRequest = AspNetNull.Persistance.Models.Claims.Requests;

namespace AspNetNull.WebAPI.Controllers.Admin
{
    [Authorize(Policy = Constants.Policies.Administrator)]
    public class PolicyController : ApiControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<LoginController> logger;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly IUnitOfWork unitOfWork;

        public PolicyController(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<LoginController> logger,
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

        [HttpPost, Route("policy")]
        public async Task<IActionResult> CreatePolicy([FromBody] PolicyClaimsMappingRequest policyRequest)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogDebug($"{nameof(this.CreatePolicy)} : Processing started.");

                    var user = await this.GetApplicationUser();
                    this.GenerateCreateRequestValues(policyRequest, user);
                    
                    policyRequest.PolicyId = policyRequest.PolicyId.GenerateUniqueId();

                    this.ValidationExecution<PolicyClaimsMappingRequest>(policyRequest);

                    this.logger.LogDebug($"{nameof(this.CreatePolicy)} : Policy creation initiated with id : {policyRequest.PolicyId}");

                    IList<PolicyClaimsRequest.ClaimRequest> masterClaims = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<PolicyClaimsRequest.ClaimRequest>>(policyRequest.ClaimsIds.Replace("\r\n", "").Replace("\\", ""));

                    await this.unitOfWork.PolicyClaimsMapping.AddAsync(policyRequest);
                    this.unitOfWork.Save();
                    return Ok();
                }, nameof(this.CreatePolicy));
        }

        [HttpGet, Route("policy/{id:length(36)}")]
        public async Task<IActionResult> GetPolicyClaimsById(string id)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogDebug($"{nameof(this.GetPolicyClaimsById)} : Procesisng started for id {id}.");

                    var policy = await this.unitOfWork.PolicyClaimsMapping.GetFirstOrDefaultAsync(p => p.PolicyId == id);
                    policy.ClaimsIds = policy.ClaimsIds.Replace("\r\n", "").Replace("\\", "");

                    var policyClaims = new PolicyClaimsMappingResponse()
                    {
                        Name = policy.Name,
                        ConcurrencyStamp = policy.ConcurrencyStamp,
                        CreatedAt = policy.CreatedAt,
                        CreatedBy = policy.CreatedBy,
                        PolicyId = policy.PolicyId,
                        UpdatedAt = policy.UpdatedAt,
                        UpdatedBy = policy.UpdatedBy,
                        ClaimsIds = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<Claims>>(policy.ClaimsIds),
                        IsDisabled = policy.IsDisabled
                    };

                    this.AddOffsetToTimeFrames(policyClaims, await this.GetApplicationUser());

                    return Ok(policyClaims);
                }, nameof(this.GetPolicyClaimsById));
        }

        [HttpGet, Route("policy")]
        public async Task<IActionResult> GetPolicyClaims()
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogDebug($"{nameof(this.GetPolicyClaims)} : Processing started.");

                    var user = await this.GetApplicationUser();
                    var policies = await this.unitOfWork.PolicyClaimsMapping.GetAllAsync();
                    var policyClaims = new List<PolicyClaimsMappingResponse>();

                    foreach (var policy in policies)
                    {
                        var policyClaim = new PolicyClaimsMappingResponse()
                        {
                            Name = policy.Name,
                            ConcurrencyStamp = policy.ConcurrencyStamp,
                            CreatedAt = policy.CreatedAt,
                            CreatedBy = policy.CreatedBy,
                            PolicyId = policy.PolicyId,
                            UpdatedAt = policy.UpdatedAt,
                            UpdatedBy = policy.UpdatedBy,
                            ClaimsIds = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<Claims>>(policy.ClaimsIds.Replace("\r\n", "").Replace("\\", "")),
                            IsDisabled = policy.IsDisabled
                        };
                        this.AddOffsetToTimeFrames(policyClaim, user);

                        policyClaims.Add(policyClaim);
                    }

                    this.logger.LogDebug($"{nameof(this.GetPolicyClaims)} : Total policies found : {policyClaims.Count}");

                    return Ok(policyClaims);
                }, nameof(this.GetPolicyClaims));
        }

        [HttpPut, Route("policy")]
        public async Task<IActionResult> UpdatePolicyClaims([FromBody] PolicyClaimsMappingRequest policyRequest)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    return await ModifyPolicyClaims(policyRequest, false);
                }, nameof(this.UpdatePolicyClaims));
        }

        [HttpDelete, Route("policy")]
        public async Task<IActionResult> DisablePolicyClaims([FromBody] PolicyClaimsMappingRequest policyRequest)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    return await ModifyPolicyClaims(policyRequest, true);
                }, nameof(this.DisablePolicyClaims));
        }

        private async Task<IActionResult> ModifyPolicyClaims([FromBody] PolicyClaimsMappingRequest policyRequest, bool isDisabled)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.logger.LogDebug($"{nameof(this.ModifyPolicyClaims)} : Processing started.");

                    this.logger.LogDebug($"{nameof(this.ModifyPolicyClaims)} : isDisabled : {isDisabled}.");

                    this.ValidationExecution<PolicyClaimsMappingRequest>(policyRequest);

                    var user = await this.GetApplicationUser();
                    var policy = await this.unitOfWork.PolicyClaimsMapping.GetFirstOrDefaultAsync(p => p.PolicyId == policyRequest.PolicyId);

                    if (policy == null)
                    {
                        return BadRequest("No policy found for.");
                    }

                    this.logger.LogDebug($"{nameof(this.ModifyPolicyClaims)} : Modifying policy : {policyRequest.PolicyId} name : {policyRequest.Name}.");

                    policy.IsDisabled = isDisabled;

                    this.GenerateUpdateRequestValues(policy, user);

                    this.unitOfWork.PolicyClaimsMapping.UpdateAsync(policyRequest);
                    this.unitOfWork.Save();

                    return Ok();
                });
        }
    }
}
