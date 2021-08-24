using AspNetNull.Persistance.Models.Claims.Requests;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Repository.IRepository
{
    public interface IClaimRepository : IRepositoryAsync<ClaimRequest>
    {
        Task<ClaimRequest> UpdateAsync(ClaimRequest claim);

        Task UpdateRangeAsync(ClaimRequest[] claims);
    }
}
