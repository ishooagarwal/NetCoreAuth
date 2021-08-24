using AspNetNull.Persistance.Data;
using AspNetNull.Persistance.Helpers;
using AspNetNull.Persistance.Models.Claims.Requests;
using AspNetNull.Persistance.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Repository
{
    public class ClaimRepository : RepositoryAsync<ClaimRequest>, IClaimRepository
    {
        private readonly ApplicationDBContext _db;
        
        public ClaimRepository(ApplicationDBContext db)
            : base(db)
        {
            this._db = db;
        }

        public async Task<ClaimRequest> UpdateAsync(ClaimRequest claim)
        {
            await this._db.HandleUpdateTasks(claim, claim.ConcurrencyStamp);

            claim.ConcurrencyStamp = ProjectHelpers.GenerateUniqueId(null);

            var result = this._db.Update(claim);

            return result.Entity;
        }

        public async Task UpdateRangeAsync(ClaimRequest[] claims)
        {
            foreach (ClaimRequest claim in claims)
            {
                await this._db.HandleUpdateTasks(claim, claim.ConcurrencyStamp);

                claim.ConcurrencyStamp = ProjectHelpers.GenerateUniqueId(null);

                var result = this._db.Update(claim);
            }
        }
    }
}
