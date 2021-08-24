using AutoMapper;
using AspNetNull.Persistance.Helpers;
using AspNetNull.Persistance.Models.Application;
using AspNetNull.Persistance.Models.ErrorResponses;
using AspNetNull.Persistance.Models.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetNull.WebAPI.Controllers
{
    [Authorize(Policy = Constants.Policies.Administrator)]
    public class RolesController : ApiControllerBase
    {
        private readonly ILogger<LoginController> logger;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IMapper mapper;
        public RolesController(RoleManager<ApplicationRole> roleManager,
            IMapper mapper,
            ILogger<LoginController> logger)
            : base(logger)
        {
            this.logger = logger;
            this.roleManager = roleManager;
            this.mapper = mapper;
        }

        [HttpPost, Route("role")]
        public async Task<IActionResult> CreateAsync([FromBody] ApplicationRole role)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.ValidationExecution<ApplicationRole>(role);
                    role.Id.GenerateUniqueId();
                    var identityResult = await this.roleManager.CreateAsync(role);
                    return await this.IdentityResultResponse<CreateRoleResponse>(identityResult, "Role created");
                }, nameof(this.CreateAsync));
        }


        [HttpGet, Route("roles")]
        public async Task<IActionResult> GetAsync()
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    var request = HttpContext.Request;
                    var roles = this.roleManager.Roles.ToList();
                    RetrieveRolesResponse rolesResponse = new RetrieveRolesResponse() { roles = roles, HttpStatusCode = System.Net.HttpStatusCode.OK };
                    return rolesResponse.ObjectResultFromStatus();

                }, nameof(this.GetAsync));
        }

        [HttpGet, Route("role/{roleId:length(36)}")]
        public async Task<IActionResult> GetAsync(string roleId)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    var role = this.roleManager.Roles.FirstOrDefault(x => x.Id == roleId);
                    RetrieveRoleResponse roleResponse = new RetrieveRoleResponse() { role = role, HttpStatusCode = System.Net.HttpStatusCode.OK };
                    return roleResponse.ObjectResultFromStatus();
                }, nameof(this.GetAsync));
        }

        [HttpPut, Route("role")]
        public async Task<IActionResult> UpdateAsync([FromBody] ApplicationRole role)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    this.ValidationExecution<ApplicationRole>(role);
                    var identityResult = await this.roleManager.UpdateAsync(role);

                    return await this.IdentityResultResponse<CreateRoleResponse>(identityResult, "Role updated");
                }, nameof(this.UpdateAsync));
        }

        [HttpDelete, Route("role/{roleId:length(36)}")]
        public async Task<IActionResult> DeleteAsync(string roleId)
        {
            return await this.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    var role = this.roleManager.Roles.FirstOrDefault(x => x.Id == roleId);
                    var identityResult = await this.roleManager.DeleteAsync(role);

                    return await this.IdentityResultResponse<CreateRoleResponse>(identityResult, "Role deleted");
                }, nameof(this.DeleteAsync));
        }
    }
}
