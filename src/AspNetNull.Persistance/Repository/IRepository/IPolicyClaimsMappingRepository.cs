using AspNetNull.Persistance.Models.Policy.Requests;

namespace AspNetNull.Persistance.Repository.IRepository
{
    public interface IPolicyClaimsMappingRepository : IRepositoryAsync<PolicyClaimsMappingRequest>
    {
        void UpdateAsync(PolicyClaimsMappingRequest policy);
    }
}
