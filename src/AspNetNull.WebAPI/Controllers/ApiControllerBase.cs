using AutoMapper;
using AspNetNull.Persistance.Helpers;
using AspNetNull.Persistance.Models;
using AspNetNull.Persistance.Models.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Validator = System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetNull.Persistance.Models.ErrorResponses;
using Microsoft.Extensions.Primitives;
using System.Net.Http;

namespace AspNetNull.WebAPI.Controllers
{
    public class ApiControllerBase : ControllerBase
    {
        private readonly ILogger logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;

        public ApiControllerBase(ILogger logger)
        {
            this.logger = logger;
        }

        public ApiControllerBase(ILogger logger, UserManager<ApplicationUser> userManager)
            :this(logger)
        {
            this.userManager = userManager;
        }

        public ApiControllerBase(ILogger logger, UserManager<ApplicationUser> userManager, IMapper mapper)
            : this(logger, userManager)
        {
            this.mapper = mapper;
        }

        public async Task<IActionResult> ExecuteWithErrorHandlingAsync(
            Func<Task<IActionResult>> operation,
            [CallerMemberName] string callerMemberName = null)
        {
            StringValues corelationId = string.Empty;
            if (!HttpContext.Request.Headers.TryGetValue("Co-relationId", out corelationId))
            {
                corelationId = Guid.NewGuid().ToString();
                this.logger.LogInformation($"Co-relationId not found in header. hence creating new GUID for request : {corelationId}");
            }

            using (this.logger.BeginScope($"{{callerMemberName}} :: {{corelationId.ToString()}}", callerMemberName, corelationId.ToString()))
            {
                try
                {
                    this.logger.LogInformation("Request processing started.");
                    var result = await operation();
                    this.logger.LogInformation("Request processing finished.");
                    return result;
                }
                catch (Validator.ValidationException ex)
                {
                    this.logger.LogError(ex, $"Validation error: {ex.Message}");
                    return ex.GetValidationError().ObjectResultFromStatus();
                }
                catch (HttpRequestException ex)
                {
                    this.logger.LogError(ex, $"Bad request : {ex.Message}");
                    return ProjectHelpers.GenericError(ex, HttpStatusCode.BadRequest).ObjectResultFromStatus();
                }
                catch (UnauthorizedAccessException ex)
                {
                    this.logger.LogError(ex, $"Unauthorized error: {ex.Message}");
                    return ex.GetUnAuthorizedError().ObjectResultFromStatus();
                }
                catch (ArgumentNullException ex)
                {
                    this.logger.LogError(ex, $"Bad request : {ex.Message}");
                    return ProjectHelpers.GenericError(ex, HttpStatusCode.BadRequest).ObjectResultFromStatus();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    this.logger.LogError(ex, $"Concurrency error: {ex.Message}");
                    return ProjectHelpers.GenericError(ex, HttpStatusCode.Conflict).ObjectResultFromStatus();
                }
                catch (DbUpdateException ex)
                {
                    this.logger.LogError(ex, $"Concurrency error: {ex.Message}");
                    if (ex.InnerException is SqlException sqlException)
                    {
                        switch (sqlException.Number)
                        {
                            case 2627:  // Unique constraint error
                            case 547:   // Constraint check violation
                            case 2601:  // Duplicated key row error
                                        // Constraint violation exception
                                        // A custom exception of yours for concurrency issues
                                return ProjectHelpers.GenericError(sqlException, HttpStatusCode.BadRequest).ObjectResultFromStatus();
                            default:
                                // A custom exception of yours for other DB issues
                                return ex.GetInternalServerError().ObjectResultFromStatus();
                        }
                    }
                    return ProjectHelpers.GenericError(ex, HttpStatusCode.BadRequest).ObjectResultFromStatus();
                }
                catch (SqlException ex)
                {
                    this.logger.LogError(ex, $"Database error: {ex.Message}");
                    return ex.GetInternalServerError().ObjectResultFromStatus();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Unexpected error: {ex.Message}");
                    return ex.GetInternalServerError().ObjectResultFromStatus();
                }
            }
        }

        public T ValidationExecution<T>(T model)
        {
            if (model == null)
            {
                throw new Validator.ValidationException("No data passed.");
            }

            Validator.Validator.ValidateObject(model, new Validator.ValidationContext(model), true);
            return model;
        }

        public async Task<ApplicationUser> GetApplicationUser()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            return await this.userManager.FindByIdAsync(claim.Value);
        }

        public void AddOffsetToTimeFrames(ModelResponse model, ApplicationUser user)
        {
            model?.CreatedAt.AddSeconds(user.Offset);
            if (!string.IsNullOrEmpty(model.UpdatedBy))
            {
                model?.UpdatedAt.AddSeconds(user.Offset);
            }
        }

        public void GenerateCreateRequestValues(Request model, ApplicationUser user)
        {
            model.CreatedBy = user.Id;
            model.CreatedAt = DateTime.UtcNow;
            model.ConcurrencyStamp = model.ConcurrencyStamp.GenerateUniqueId();
        }

        public void GenerateUpdateRequestValues(Request model, ApplicationUser user)
        {
            model.UpdatedBy = user.Id;
            model.UpdatedAt = DateTime.UtcNow;
        }

        public async Task<IActionResult> IdentityResultResponse<TResponse>(IdentityResult identityResult, string message, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest) where TResponse : Response, new()
        {
            if (identityResult.Succeeded)
            {
                return new TResponse() { Message = message, HttpStatusCode = HttpStatusCode.OK }.ObjectResultFromStatus();
            }
            else
            {
                var errors = this.mapper.Map<Errors>(identityResult.Errors);
                errors.HttpStatusCode = httpStatusCode;
                return errors.ObjectResultFromStatus();
            }
        }
    }
}
