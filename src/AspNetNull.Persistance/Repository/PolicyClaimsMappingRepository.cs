using AspNetNull.Persistance.Data;
using AspNetNull.Persistance.Helpers;
using AspNetNull.Persistance.Models.Policy.Requests;
using AspNetNull.Persistance.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Repository
{
    public class PolicyClaimsMappingRepository : RepositoryAsync<PolicyClaimsMappingRequest>, IPolicyClaimsMappingRepository
    {
        private readonly ApplicationDBContext _db;
        public PolicyClaimsMappingRepository(ApplicationDBContext db)
            :base(db)
        {
            this._db = db;
        }

        public async void UpdateAsync(PolicyClaimsMappingRequest policy)
        {
            await this._db.HandleUpdateTasks(policy, policy.ConcurrencyStamp);

            policy.ConcurrencyStamp.GenerateUniqueId();
            var result = this._db.Update(policy);
        }
    }
}
