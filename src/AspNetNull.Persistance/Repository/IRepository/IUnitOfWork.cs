using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IPolicyClaimsMappingRepository PolicyClaimsMapping { get; }

        IClaimRepository ClaimRepository { get; }

        void Save();
    }
}
