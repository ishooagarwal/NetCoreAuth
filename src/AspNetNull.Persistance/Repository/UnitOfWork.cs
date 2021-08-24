using AspNetNull.Persistance.Data;
using AspNetNull.Persistance.Repository.IRepository;

namespace AspNetNull.Persistance.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDBContext _db;
        
        public UnitOfWork(ApplicationDBContext db)
        {
            this._db = db;
            PolicyClaimsMapping = new PolicyClaimsMappingRepository(_db);
            ClaimRepository = new ClaimRepository(_db);
        }

        public IPolicyClaimsMappingRepository PolicyClaimsMapping { get; private set; }
        public IClaimRepository ClaimRepository { get; private set; }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
