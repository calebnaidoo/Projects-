using GLMS.API.Data;
using GLMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Repositories
{
    // DESIGN PATTERN 1: REPOSITORY PATTERN (carried from Part 2 into API layer)
    // All SQL queries live here. Controllers only call repository interfaces.

    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetAllAsync();
        Task<Client?> GetByIdAsync(int id);
        Task<Client> CreateAsync(Client c);
        Task UpdateAsync(Client c);
        Task DeleteAsync(int id);
    }

    public class ClientRepository : IClientRepository
    {
        private readonly ApiDbContext _db;
        public ClientRepository(ApiDbContext db) => _db = db;
        public async Task<IEnumerable<Client>> GetAllAsync() =>
            await _db.Clients.Include(c => c.Contracts).OrderBy(c => c.Name).ToListAsync();
        public async Task<Client?> GetByIdAsync(int id) =>
            await _db.Clients.Include(c => c.Contracts).FirstOrDefaultAsync(c => c.Id == id);
        public async Task<Client> CreateAsync(Client c) { _db.Clients.Add(c); await _db.SaveChangesAsync(); return c; }
        public async Task UpdateAsync(Client c) { _db.Clients.Update(c); await _db.SaveChangesAsync(); }
        public async Task DeleteAsync(int id)
        { var c = await _db.Clients.FindAsync(id); if (c != null) { _db.Clients.Remove(c); await _db.SaveChangesAsync(); } }
    }

    public interface IContractRepository
    {
        Task<IEnumerable<Contract>> GetAllAsync(DateTime? from, DateTime? to, ContractStatus? status, string? search);
        Task<Contract?> GetByIdAsync(int id);
        Task<Contract> CreateAsync(Contract c);
        Task UpdateAsync(Contract c);
        Task DeleteAsync(int id);
    }

    public class ContractRepository : IContractRepository
    {
        private readonly ApiDbContext _db;
        public ContractRepository(ApiDbContext db) => _db = db;
        public async Task<IEnumerable<Contract>> GetAllAsync(DateTime? from, DateTime? to, ContractStatus? status, string? search)
        {
            var q = _db.Contracts.Include(c => c.Client).Include(c => c.ServiceRequests).AsQueryable();
            if (from.HasValue)   q = q.Where(c => c.StartDate >= from.Value);
            if (to.HasValue)     q = q.Where(c => c.StartDate <= to.Value);
            if (status.HasValue) q = q.Where(c => c.Status == status.Value);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(c => c.Title.Contains(search) || c.Client!.Name.Contains(search));
            return await q.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }
        public async Task<Contract?> GetByIdAsync(int id) =>
            await _db.Contracts.Include(c => c.Client).Include(c => c.ServiceRequests).FirstOrDefaultAsync(c => c.Id == id);
        public async Task<Contract> CreateAsync(Contract c) { _db.Contracts.Add(c); await _db.SaveChangesAsync(); return c; }
        public async Task UpdateAsync(Contract c) { _db.Contracts.Update(c); await _db.SaveChangesAsync(); }
        public async Task DeleteAsync(int id)
        { var c = await _db.Contracts.FindAsync(id); if (c != null) { _db.Contracts.Remove(c); await _db.SaveChangesAsync(); } }
    }

    public interface IServiceRequestRepository
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
        Task<IEnumerable<ServiceRequest>> GetByContractAsync(int contractId);
        Task<ServiceRequest?> GetByIdAsync(int id);
        Task<ServiceRequest> CreateAsync(ServiceRequest r);
        Task DeleteAsync(int id);
    }

    public class ServiceRequestRepository : IServiceRequestRepository
    {
        private readonly ApiDbContext _db;
        public ServiceRequestRepository(ApiDbContext db) => _db = db;
        public async Task<IEnumerable<ServiceRequest>> GetAllAsync() =>
            await _db.ServiceRequests.Include(r => r.Contract).ThenInclude(c => c!.Client).OrderByDescending(r => r.DateRaised).ToListAsync();
        public async Task<IEnumerable<ServiceRequest>> GetByContractAsync(int contractId) =>
            await _db.ServiceRequests.Where(r => r.ContractId == contractId).OrderByDescending(r => r.DateRaised).ToListAsync();
        public async Task<ServiceRequest?> GetByIdAsync(int id) =>
            await _db.ServiceRequests.Include(r => r.Contract).ThenInclude(c => c!.Client).FirstOrDefaultAsync(r => r.Id == id);
        public async Task<ServiceRequest> CreateAsync(ServiceRequest r) { _db.ServiceRequests.Add(r); await _db.SaveChangesAsync(); return r; }
        public async Task DeleteAsync(int id)
        { var r = await _db.ServiceRequests.FindAsync(id); if (r != null) { _db.ServiceRequests.Remove(r); await _db.SaveChangesAsync(); } }
    }
}
