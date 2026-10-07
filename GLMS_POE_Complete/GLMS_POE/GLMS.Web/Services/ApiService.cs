using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GLMS.Web.Services
{
    // ─────────────────────────────────────────────────────────────────────────
    // PART 3 ADDITION: ApiService
    // ─────────────────────────────────────────────────────────────────────────
    // The MVC frontend can now call the GLMS.API backend via HttpClient.
    // This is the Separation of Concerns required for Part 3:
    //   - Part 2 controllers still use DbContext directly (monolith preserved)
    //   - Part 3 demonstrates the decoupled SOA approach via this service
    // ─────────────────────────────────────────────────────────────────────────

    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;
        private readonly ILogger<ApiService> _logger;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public ApiService(HttpClient http, IHttpContextAccessor ctx, ILogger<ApiService> logger)
        {
            _http   = http;
            _ctx    = ctx;
            _logger = logger;
        }

        // Attach JWT from session if available
        private void AttachToken()
        {
            var token = _ctx.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            else
                _http.DefaultRequestHeaders.Authorization = null;
        }

        private async Task<T?> GetAsync<T>(string url)
        {
            AttachToken();
            try
            {
                var r = await _http.GetAsync(url);
                if (!r.IsSuccessStatusCode) return default;
                return JsonSerializer.Deserialize<T>(await r.Content.ReadAsStringAsync(), _json);
            }
            catch (Exception ex) { _logger.LogError(ex, "GET {Url} failed", url); return default; }
        }

        private async Task<(T? Result, string? Error)> PostAsync<T>(string url, object body)
        {
            AttachToken();
            try
            {
                var content  = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var r        = await _http.PostAsync(url, content);
                var text     = await r.Content.ReadAsStringAsync();
                if (!r.IsSuccessStatusCode) return (default, text);
                return (JsonSerializer.Deserialize<T>(text, _json), null);
            }
            catch (Exception ex) { return (default, ex.Message); }
        }

        private async Task<(bool Ok, string? Error)> PatchAsync(string url, object body)
        {
            AttachToken();
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var req     = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
                var r       = await _http.SendAsync(req);
                return r.IsSuccessStatusCode ? (true, null) : (false, await r.Content.ReadAsStringAsync());
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        private async Task<bool> DeleteAsync(string url)
        {
            AttachToken();
            try { return (await _http.DeleteAsync(url)).IsSuccessStatusCode; }
            catch { return false; }
        }

        // ── AUTH ──────────────────────────────────────────────────────────────
        public async Task<(string? Token, string? Error)> ApiLoginAsync(string email, string password)
        {
            var (result, error) = await PostAsync<ApiAuthResponse>("api/auth/login", new { email, password });
            return (result?.Token, error);
        }

        // ── CONTRACTS ────────────────────────────────────────────────────────
        public Task<List<ApiContractDto>?> ApiGetContractsAsync(
            DateTime? from = null, DateTime? to = null, string? status = null, string? search = null)
        {
            var q = new List<string>();
            if (from.HasValue)  q.Add($"startFrom={from:yyyy-MM-dd}");
            if (to.HasValue)    q.Add($"startTo={to:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(status)) q.Add($"status={status}");
            if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={Uri.EscapeDataString(search)}");
            return GetAsync<List<ApiContractDto>>("api/contracts" + (q.Count > 0 ? "?" + string.Join("&", q) : ""));
        }

        public Task<ApiContractDto?> ApiGetContractAsync(int id)
            => GetAsync<ApiContractDto>($"api/contracts/{id}");

        public Task<(ApiContractDto? Result, string? Error)> ApiCreateContractAsync(object dto)
            => PostAsync<ApiContractDto>("api/contracts", dto);

        public Task<(bool Ok, string? Error)> ApiPatchStatusAsync(int id, string status)
            => PatchAsync($"api/contracts/{id}/status", new { status });

        public Task<bool> ApiDeleteContractAsync(int id) => DeleteAsync($"api/contracts/{id}");

        // ── SERVICE REQUESTS ──────────────────────────────────────────────────
        public Task<List<ApiServiceRequestDto>?> ApiGetServiceRequestsAsync()
            => GetAsync<List<ApiServiceRequestDto>>("api/servicerequests");

        public Task<(ApiServiceRequestDto? Result, string? Error)> ApiCreateServiceRequestAsync(object dto)
            => PostAsync<ApiServiceRequestDto>("api/servicerequests", dto);

        // ── CURRENCY ──────────────────────────────────────────────────────────
        public Task<ApiCurrencyRatesDto?> ApiGetRatesAsync()
            => GetAsync<ApiCurrencyRatesDto>("api/currency/rates");
    }

    // ── Response shapes (used only by ApiService, not bound to views) ─────────
    public class ApiAuthResponse  { public string Token { get; set; } = ""; public string Email { get; set; } = ""; }
    public class ApiContractDto
    {
        public int Id { get; set; } public string Title { get; set; } = "";
        public string ClientName { get; set; } = ""; public string Status { get; set; } = "";
        public string ServiceLevel { get; set; } = ""; public decimal ContractValueUSD { get; set; }
        public DateTime StartDate { get; set; } public DateTime EndDate { get; set; }
        public string? Notes { get; set; } public bool HasAgreement { get; set; }
        public int ServiceRequestCount { get; set; }
    }
    public class ApiServiceRequestDto
    {
        public int Id { get; set; } public string Description { get; set; } = "";
        public decimal CostZAR { get; set; } public string Status { get; set; } = "";
    }
    public class ApiCurrencyRatesDto
    {
        public decimal UsdToZar { get; set; } public decimal EurToZar { get; set; }
        public decimal GbpToZar { get; set; } public bool IsLive { get; set; }
    }
}
