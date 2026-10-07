using System.Text.Json;

namespace GLMS.Web.Services
{
    // ─────────────────────────────────────────────────────────────────────────
    // CURRENCY SERVICE — fetches live exchange rates from ExchangeRate-API v6.
    // Supports USD, EUR, GBP and any other currency the API provides.
    // Rates are cached for 1 hour to preserve API quota.
    // Falls back to hardcoded rates if the API is unreachable.
    // ─────────────────────────────────────────────────────────────────────────

    public interface ICurrencyService
    {
        /// <summary>Gets live rates for all supported currencies relative to ZAR.</summary>
        Task<CurrencyRates> GetRatesAsync();

        /// <summary>Converts any supported currency amount to ZAR.</summary>
        decimal ConvertToZar(decimal amount, string currency, CurrencyRates rates);

        /// <summary>Returns supported currency codes and their display labels.</summary>
        Dictionary<string, string> GetSupportedCurrencies();
    }

    /// <summary>Holds current exchange rates to ZAR for supported currencies.</summary>
    public class CurrencyRates
    {
        public decimal UsdToZar { get; set; } = 18.50m;
        public decimal EurToZar { get; set; } = 20.10m;
        public decimal GbpToZar { get; set; } = 23.40m;
        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
        public bool IsLive { get; set; } = false;

        /// <summary>Returns the ZAR rate for the given currency code.</summary>
        public decimal GetRate(string currency) => currency.ToUpper() switch
        {
            "USD" => UsdToZar,
            "EUR" => EurToZar,
            "GBP" => GbpToZar,
            _ => UsdToZar // fallback to USD rate for unknown currencies
        };
    }

    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IConfiguration _configuration;

        // Static cache shared across all requests — refreshed every hour
        private static CurrencyRates? _cachedRates;
        private static DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly object _lock = new();

        public CurrencyService(
            HttpClient httpClient,
            ILogger<CurrencyService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Returns supported currency codes mapped to their display names.
        /// Used to populate the currency dropdown on the service request form.
        /// </summary>
        public Dictionary<string, string> GetSupportedCurrencies() => new()
        {
            { "USD", "🇺🇸 USD — US Dollar" },
            { "EUR", "🇪🇺 EUR — Euro" },
            { "GBP", "🇬🇧 GBP — British Pound" }
        };

        /// <summary>
        /// Fetches live USD-based rates from ExchangeRate-API v6, then derives
        /// ZAR rates for EUR and GBP by cross-multiplying.
        /// Returns cached rates if still valid (within 1-hour window).
        /// </summary>
        public async Task<CurrencyRates> GetRatesAsync()
        {
            // Return cached rates if still valid
            lock (_lock)
            {
                if (_cachedRates != null && DateTime.UtcNow < _cacheExpiry)
                {
                    _logger.LogInformation("Returning cached exchange rates (expires {Expiry})", _cacheExpiry);
                    return _cachedRates;
                }
            }

            try
            {
                var apiKey = _configuration["ExchangeRateApi:ApiKey"];
                var baseUrl = _configuration["ExchangeRateApi:BaseUrl"];

                if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_API_KEY_HERE")
                {
                    _logger.LogWarning("API key not configured — using fallback rates.");
                    return GetFallbackRates();
                }

                // Fetch USD-based rates: all currencies relative to 1 USD
                var url = $"{baseUrl}/{apiKey}/latest/USD";
                _logger.LogInformation("Fetching live rates from ExchangeRate-API...");

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);

                // Validate API response status
                if (json.RootElement.GetProperty("result").GetString() != "success")
                {
                    _logger.LogWarning("API returned non-success status — using fallback.");
                    return GetFallbackRates();
                }

                var convRates = json.RootElement.GetProperty("conversion_rates");

                // Extract ZAR rate (how many ZAR per 1 USD)
                var usdToZar = convRates.GetProperty("ZAR").GetDecimal();

                // Derive EUR→ZAR and GBP→ZAR:
                // If 1 USD = X ZAR and 1 USD = Y EUR, then 1 EUR = X/Y ZAR
                var eurRate = convRates.GetProperty("EUR").GetDecimal();
                var gbpRate = convRates.GetProperty("GBP").GetDecimal();

                var eurToZar = Math.Round(usdToZar / eurRate, 4);
                var gbpToZar = Math.Round(usdToZar / gbpRate, 4);

                var rates = new CurrencyRates
                {
                    UsdToZar = usdToZar,
                    EurToZar = eurToZar,
                    GbpToZar = gbpToZar,
                    FetchedAt = DateTime.UtcNow,
                    IsLive = true
                };

                // Cache for 1 hour
                lock (_lock)
                {
                    _cachedRates = rates;
                    _cacheExpiry = DateTime.UtcNow.AddHours(1);
                }

                _logger.LogInformation(
                    "Live rates: USD={Usd}, EUR={Eur}, GBP={Gbp} (all to ZAR)",
                    usdToZar, eurToZar, gbpToZar);

                return rates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch rates — using fallback.");
                return GetFallbackRates();
            }
        }

        /// <summary>
        /// Converts a given amount in the specified currency to ZAR.
        /// Uses the rates fetched from the API (or fallback rates).
        /// </summary>
        public decimal ConvertToZar(decimal amount, string currency, CurrencyRates rates)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            var rate = rates.GetRate(currency);

            if (rate <= 0)
                throw new ArgumentException("Exchange rate must be greater than zero.", nameof(currency));

            return Math.Round(amount * rate, 2);
        }

        /// <summary>
        /// Fallback rates used when the API is unavailable.
        /// Approximate market rates at time of development.
        /// </summary>
        private static CurrencyRates GetFallbackRates() => new()
        {
            UsdToZar = 18.50m,
            EurToZar = 20.10m,
            GbpToZar = 23.40m,
            FetchedAt = DateTime.UtcNow,
            IsLive = false
        };
    }
}
