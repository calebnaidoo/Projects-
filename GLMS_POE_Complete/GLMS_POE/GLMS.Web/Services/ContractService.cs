using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Services
{
    // ═══════════════════════════════════════════════════════════════════════
    //
    //  DESIGN PATTERN 1: REPOSITORY PATTERN
    //  ─────────────────────────────────────
    //  Abstracts all EF Core data access behind an interface.
    //  Controllers never touch DbContext directly — they call the repository.
    //  This separates the data access layer from the business logic layer,
    //  making the code easier to test and maintain.
    //
    // ═══════════════════════════════════════════════════════════════════════

    public interface IContractRepository
    {
        Task<IEnumerable<Contract>> GetAllAsync();
        Task<Contract?> GetByIdAsync(int id);
        Task<IEnumerable<Contract>> FilterAsync(DateTime? from, DateTime? to, ContractStatus? status, string? search);
        Task<Contract> CreateAsync(Contract contract);
        Task UpdateAsync(Contract contract);
        Task DeleteAsync(int id);
    }

    /// <summary>
    /// Concrete repository implementation using EF Core.
    /// All queries include related entities (Client, ServiceRequests)
    /// to avoid N+1 query problems in views.
    /// </summary>
    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext _db;

        public ContractRepository(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<Contract>> GetAllAsync() =>
            await _db.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task<Contract?> GetByIdAsync(int id) =>
            await _db.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);

        /// <summary>
        /// LINQ-based filter used on the Contracts Index page.
        /// Supports filtering by date range, status, and free-text search.
        /// Uses deferred execution — conditions are chained before hitting the DB.
        /// </summary>
        public async Task<IEnumerable<Contract>> FilterAsync(
            DateTime? from, DateTime? to, ContractStatus? status, string? search)
        {
            var query = _db.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(c => c.StartDate >= from.Value);

            if (to.HasValue)
                query = query.Where(c => c.StartDate <= to.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c =>
                    c.Title.Contains(search) ||
                    c.Client!.Name.Contains(search));

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<Contract> CreateAsync(Contract contract)
        {
            _db.Contracts.Add(contract);
            await _db.SaveChangesAsync();
            return contract;
        }

        public async Task UpdateAsync(Contract contract)
        {
            _db.Contracts.Update(contract);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var c = await _db.Contracts.FindAsync(id);
            if (c != null) { _db.Contracts.Remove(c); await _db.SaveChangesAsync(); }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //
    //  DESIGN PATTERN 2: FACTORY PATTERN
    //  ──────────────────────────────────
    //  Centralises ServiceRequest creation logic.
    //  Instead of controllers manually constructing ServiceRequest objects,
    //  the factory handles validation and ZAR conversion in one place.
    //  This ensures every request is created consistently.
    //
    // ═══════════════════════════════════════════════════════════════════════

    public interface IServiceRequestFactory
    {
        /// <summary>
        /// Creates a validated ServiceRequest with automatic ZAR conversion.
        /// Throws ArgumentException if cost or rate is invalid.
        /// </summary>
        ServiceRequest Create(
            int contractId, string description, string currency,
            decimal originalCost, decimal rateToZar,
            string priority, string requestedBy);
    }

    public class ServiceRequestFactory : IServiceRequestFactory
    {
        /// <summary>
        /// Factory method — validates inputs and constructs a ServiceRequest.
        /// The ZAR amount is calculated here so the controller stays clean.
        /// </summary>
        public ServiceRequest Create(
            int contractId, string description, string currency,
            decimal originalCost, decimal rateToZar,
            string priority, string requestedBy)
        {
            if (originalCost <= 0)
                throw new ArgumentException("Cost must be greater than zero.", nameof(originalCost));

            if (rateToZar <= 0)
                throw new ArgumentException("Exchange rate must be greater than zero.", nameof(rateToZar));

            return new ServiceRequest
            {
                ContractId = contractId,
                Description = description,
                Currency = currency.ToUpper(),
                OriginalCost = originalCost,
                CostZAR = Math.Round(originalCost * rateToZar, 2),
                ExchangeRateToZAR = rateToZar,
                Priority = priority,
                RequestedBy = requestedBy,
                Status = ServiceRequestStatus.Pending,
                DateRaised = DateTime.UtcNow
            };
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //
    //  DESIGN PATTERN 3: OBSERVER PATTERN
    //  ─────────────────────────────────────
    //  Decouples contract status change notifications from the service.
    //  When a contract status changes, all registered observers are notified.
    //  New observers can be added without modifying ContractService.
    //  Currently used for audit logging; can be extended for email alerts.
    //
    // ═══════════════════════════════════════════════════════════════════════

    public interface IContractObserver
    {
        void OnStatusChanged(Contract contract, ContractStatus oldStatus, ContractStatus newStatus);
    }

    /// <summary>
    /// Concrete observer that logs all contract status transitions.
    /// Provides an audit trail visible in the application logs.
    /// </summary>
    public class ContractStatusLogger : IContractObserver
    {
        private readonly ILogger<ContractStatusLogger> _logger;

        public ContractStatusLogger(ILogger<ContractStatusLogger> logger)
            => _logger = logger;

        public void OnStatusChanged(Contract contract, ContractStatus old, ContractStatus @new)
            => _logger.LogInformation(
                "[AUDIT] Contract [{Id}] '{Title}': {Old} → {New} at {Time} UTC",
                contract.Id, contract.Title, old, @new, DateTime.UtcNow);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CONTRACT SERVICE — business rules + observer wiring
    // ─────────────────────────────────────────────────────────────────────────

    public interface IContractService
    {
        /// <summary>
        /// Core business rule: only Active or Draft contracts can receive requests.
        /// Returns false for Expired and OnHold contracts.
        /// </summary>
        Task<bool> CanRaiseServiceRequestAsync(int contractId);

        /// <summary>Updates the contract status and notifies all observers.</summary>
        Task UpdateStatusAsync(int contractId, ContractStatus newStatus);

        /// <summary>Registers an observer to be notified on status changes.</summary>
        void RegisterObserver(IContractObserver observer);
    }

    public class ContractService : IContractService
    {
        private readonly ApplicationDbContext _db;
        private readonly List<IContractObserver> _observers = new();

        public ContractService(ApplicationDbContext db) => _db = db;

        public void RegisterObserver(IContractObserver observer)
            => _observers.Add(observer);

        // Notifies all registered observers — Observer Pattern notification
        private void Notify(Contract c, ContractStatus old, ContractStatus @new)
        {
            foreach (var observer in _observers)
                observer.OnStatusChanged(c, old, @new);
        }

        /// <summary>
        /// BUSINESS RULE: Service requests can only be raised on Active or Draft contracts.
        /// Expired and OnHold contracts are blocked at this service layer,
        /// not just in the UI, ensuring the rule cannot be bypassed.
        /// </summary>
        public async Task<bool> CanRaiseServiceRequestAsync(int contractId)
        {
            var contract = await _db.Contracts.FindAsync(contractId);
            if (contract == null) return false;

            return contract.Status == ContractStatus.Active ||
                   contract.Status == ContractStatus.Draft;
        }

        /// <summary>
        /// Updates the contract status and fires the observer notification.
        /// Throws KeyNotFoundException if the contract doesn't exist.
        /// </summary>
        public async Task UpdateStatusAsync(int contractId, ContractStatus newStatus)
        {
            var contract = await _db.Contracts.FindAsync(contractId)
                ?? throw new KeyNotFoundException($"Contract {contractId} not found.");

            var oldStatus = contract.Status;
            contract.Status = newStatus;
            await _db.SaveChangesAsync();

            // Observer pattern — notify all registered listeners
            Notify(contract, oldStatus, newStatus);
        }
    }
}
