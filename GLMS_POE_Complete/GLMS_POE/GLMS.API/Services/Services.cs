using GLMS.API.Data;
using GLMS.API.Models;
using System.Text.Json;

namespace GLMS.API.Services
{
    // DESIGN PATTERN 2: FACTORY PATTERN (same as Part 2, moved into API layer)
    public interface IApiServiceRequestFactory
    {
        ServiceRequest Create(int contractId, string description, string currency,
                              decimal originalCost, decimal rateToZar, string priority, string requestedBy);
    }
    public class ApiServiceRequestFactory : IApiServiceRequestFactory
    {
        public ServiceRequest Create(int contractId, string description, string currency,
                                     decimal originalCost, decimal rateToZar, string priority, string requestedBy)
        {
            if (originalCost <= 0) throw new ArgumentException("Cost must be greater than zero.", nameof(originalCost));
            if (rateToZar    <= 0) throw new ArgumentException("Rate must be greater than zero.", nameof(rateToZar));
            return new ServiceRequest
            {
                ContractId        = contractId,
                Description       = description,
                Currency          = currency.ToUpper(),
                OriginalCost      = originalCost,
                CostZAR           = Math.Round(originalCost * rateToZar, 2),
                ExchangeRateToZAR = rateToZar,
                Priority          = priority,
                RequestedBy       = requestedBy,
                Status            = ServiceRequestStatus.Pending,
                DateRaised        = DateTime.UtcNow
            };
        }
    }

    // DESIGN PATTERN 3: OBSERVER PATTERN (same as Part 2, moved into API layer)
    public interface IContractObserver
    {
        void OnStatusChanged(Contract contract, ContractStatus oldStatus, ContractStatus newStatus);
    }
    public class ContractStatusLogger : IContractObserver
    {
        private readonly ILogger<ContractStatusLogger> _logger;
        public ContractStatusLogger(ILogger<ContractStatusLogger> logger) => _logger = logger;
        public void OnStatusChanged(Contract c, ContractStatus old, ContractStatus @new) =>
            _logger.LogInformation("[AUDIT] Contract [{Id}] '{Title}': {Old} -> {New} at {Time} UTC",
                c.Id, c.Title, old, @new, DateTime.UtcNow);
    }

    public interface IApiContractService
    {
        Task<bool> CanRaiseServiceRequestAsync(int contractId);
        Task UpdateStatusAsync(int contractId, ContractStatus newStatus);
        void RegisterObserver(IContractObserver observer);
    }
    public class ApiContractService : IApiContractService
    {
        private readonly ApiDbContext _db;
        private readonly List<IContractObserver> _observers = new();
        public ApiContractService(ApiDbContext db) => _db = db;
        public void RegisterObserver(IContractObserver o) => _observers.Add(o);
        private void Notify(Contract c, ContractStatus old, ContractStatus @new)
        { foreach (var o in _observers) o.OnStatusChanged(c, old, @new); }
        public async Task<bool> CanRaiseServiceRequestAsync(int contractId)
        {
            var c = await _db.Contracts.FindAsync(contractId);
            return c is { Status: ContractStatus.Active or ContractStatus.Draft };
        }
        public async Task UpdateStatusAsync(int contractId, ContractStatus newStatus)
        {
            var c = await _db.Contracts.FindAsync(contractId)
                ?? throw new KeyNotFoundException($"Contract {contractId} not found.");
            var old = c.Status;
            c.Status = newStatus;
            await _db.SaveChangesAsync();
            Notify(c, old, newStatus);
        }
    }

    // CURRENCY SERVICE (same logic as Part 2 CurrencyService, reused in API)
    public class ApiCurrencyRates
    {
        public decimal UsdToZar  { get; set; } = 18.50m;
        public decimal EurToZar  { get; set; } = 20.10m;
        public decimal GbpToZar  { get; set; } = 23.40m;
        public bool    IsLive    { get; set; } = false;
        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
        public decimal GetRate(string currency) => currency.ToUpper() switch
        { "EUR" => EurToZar, "GBP" => GbpToZar, _ => UsdToZar };
    }
    public interface IApiCurrencyService
    {
        Task<ApiCurrencyRates> GetRatesAsync();
        decimal ConvertToZar(decimal amount, string currency, ApiCurrencyRates rates);
    }
    public class ApiCurrencyService : IApiCurrencyService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ApiCurrencyService> _logger;
        private readonly IConfiguration _config;
        private static ApiCurrencyRates? _cache;
        private static DateTime _expiry = DateTime.MinValue;
        private static readonly object _lock = new();

        public ApiCurrencyService(HttpClient http, ILogger<ApiCurrencyService> logger, IConfiguration config)
        { _http = http; _logger = logger; _config = config; }

        public async Task<ApiCurrencyRates> GetRatesAsync()
        {
            lock (_lock) { if (_cache != null && DateTime.UtcNow < _expiry) return _cache; }
            try
            {
                var key = _config["ExchangeRateApi:ApiKey"];
                var url = _config["ExchangeRateApi:BaseUrl"];
                if (string.IsNullOrWhiteSpace(key) || key == "YOUR_API_KEY_HERE") return Fallback();
                var r = await _http.GetAsync($"{url}/{key}/latest/USD");
                r.EnsureSuccessStatusCode();
                var json = JsonDocument.Parse(await r.Content.ReadAsStringAsync());
                if (json.RootElement.GetProperty("result").GetString() != "success") return Fallback();
                var conv = json.RootElement.GetProperty("conversion_rates");
                var zarRate = conv.GetProperty("ZAR").GetDecimal();
                var eurRate = conv.GetProperty("EUR").GetDecimal();
                var gbpRate = conv.GetProperty("GBP").GetDecimal();
                var rates = new ApiCurrencyRates
                {
                    UsdToZar = zarRate, EurToZar = Math.Round(zarRate / eurRate, 4),
                    GbpToZar = Math.Round(zarRate / gbpRate, 4), IsLive = true, FetchedAt = DateTime.UtcNow
                };
                lock (_lock) { _cache = rates; _expiry = DateTime.UtcNow.AddHours(1); }
                return rates;
            }
            catch (Exception ex) { _logger.LogError(ex, "Currency API failed — using fallback."); return Fallback(); }
        }

        public decimal ConvertToZar(decimal amount, string currency, ApiCurrencyRates rates)
        {
            if (amount < 0) throw new ArgumentException("Amount cannot be negative.");
            var rate = rates.GetRate(currency);
            if (rate <= 0) throw new ArgumentException("Exchange rate must be > 0.");
            return Math.Round(amount * rate, 2);
        }
        private static ApiCurrencyRates Fallback() => new() { IsLive = false, FetchedAt = DateTime.UtcNow };
    }
}
