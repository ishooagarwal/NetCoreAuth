using AutoMapper;
using AspNetNull.Persistance.Models.Application;
using AspNetNull.Persistance.Models.ErrorResponses;
using AspNetNull.Persistance.Models.Policy;
using AspNetNull.Persistance.Models.User;
using AspNetNull.Persistance.Models.UserClaims;
using PolicyClaimsRequest = AspNetNull.Persistance.Models.Claims.Requests;
using PolicyClaimsResponse = AspNetNull.Persistance.Models.Claims.Responses;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using AspNetNull.Persistance.Models.Policy.Requests;

namespace AspNetNull.WebAPI.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            this.CreateMap<IdentityError, Error>()
                .ForMember(s => s.errorMessages, e => e.MapFrom(z => z.Description));

            this.CreateMap<IEnumerable<IdentityError>, Errors>()
                .ForMember(s => s.Error, e => e.MapFrom(z => z));

            this.CreateMap<Claim, Claims>()
                .ForMember(s => s.Name, e => e.MapFrom(z => z.Type))
                .ForMember(s => s.Value, e => e.MapFrom(z => z.Value));

            this.CreateMap<ApplicationUser, RetrieveUser>()
                .ForMember(a => a.userName, r => r.MapFrom(z => z.UserName))
                .ForMember(a => a.normalizedUserName, r => r.MapFrom(z => z.NormalizedUserName))
                .ForMember(a => a.email, r => r.MapFrom(z => z.Email))
                .ForMember(a => a.emailConfirmed, r => r.MapFrom(z => z.EmailConfirmed))
                .ForMember(a => a.normalizedEmail, r => r.MapFrom(z => z.NormalizedEmail))
                .ForMember(a => a.concurrencyStamp, r => r.MapFrom(z => z.ConcurrencyStamp))
                .ForMember(a => a.accessFailedCount, r => r.MapFrom(z => z.AccessFailedCount))
                .ForMember(a => a.lockoutEnabled, r => r.MapFrom(z => z.LockoutEnabled))
                .ForMember(a => a.lockoutEnd, r => r.MapFrom(z => z.LockoutEnd))
                .ForMember(a => a.phoneNumber, r => r.MapFrom(z => z.PhoneNumber))
                .ForMember(a => a.phoneNumberConfirmed, r => r.MapFrom(z => z.PhoneNumberConfirmed))
                .ForMember(a => a.twoFactorEnabled, r => r.MapFrom(z => z.TwoFactorEnabled));

            this.CreateMap<PolicyClaimsMappingRequest, PolicyClaimsMappingResponse>()
                .ForMember(c => c.PolicyId, p => p.MapFrom(z => z.PolicyId))
                .ForMember(c => c.Name, p => p.MapFrom(z => z.Name))
                .ForMember(c => c.CreatedAt, p => p.MapFrom(z => z.CreatedAt))
                .ForMember(c => c.CreatedBy, p => p.MapFrom(z => z.CreatedBy))
                .ForMember(c => c.UpdatedAt, p => p.MapFrom(z => z.UpdatedAt))
                .ForMember(c => c.UpdatedBy, p => p.MapFrom(z => z.UpdatedBy))
                .ForMember(c => c.ConcurrencyStamp, p => p.MapFrom(z => z.ConcurrencyStamp));

            this.CreateMap<PolicyClaimsRequest.ClaimRequest, PolicyClaimsResponse.ClaimResponse>()
                .ForMember(c => c.Id, cr => cr.MapFrom(z => z.Id))
                .ForMember(c => c.Name, cr => cr.MapFrom(z => z.Name))
                .ForMember(c => c.Value, cr => cr.MapFrom(z => z.Value))
                .ForMember(c => c.ConcurrencyStamp, cr => cr.MapFrom(z => z.ConcurrencyStamp))
                .ForMember(c => c.CreatedAt, cr => cr.MapFrom(z => z.CreatedAt))
                .ForMember(c => c.CreatedBy, cr => cr.MapFrom(z => z.CreatedBy))
                .ForMember(c => c.UpdatedAt, cr => cr.MapFrom(z => z.UpdatedAt))
                .ForMember(c => c.UpdatedBy, cr => cr.MapFrom(z => z.UpdatedBy))
                .ForMember(c => c.IsDisabled, cr => cr.MapFrom(z => z.IsDisabled));
        }
    }
}
