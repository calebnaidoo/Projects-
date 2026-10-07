using GLMS.API.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace GLMS.Tests
{
    // Custom factory — replaces SQL Server with InMemory so tests run without Docker
   public class GlmsApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApiDbContext>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<ApiDbContext>(options =>
                options.UseInMemoryDatabase("IntegrationTestDb_" + Guid.NewGuid()));
        });
    }
}

    public class ContractsIntegrationTests : IClassFixture<GlmsApiFactory>
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public ContractsIntegrationTests(GlmsApiFactory factory)
            => _client = factory.CreateClient();

        // Test 1: GET /api/contracts returns HTTP 200
        [Fact] public async Task GetContracts_Returns200()
            => Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/contracts")).StatusCode);

        // Test 2: GET /api/contracts returns non-null JSON array
        [Fact] public async Task GetContracts_ReturnsNonNullJson()
        {
            var r = await _client.GetAsync("/api/contracts");
            r.EnsureSuccessStatusCode();
            var contracts = JsonSerializer.Deserialize<List<JsonElement>>(
                await r.Content.ReadAsStringAsync(), _json);
            Assert.NotNull(contracts);
        }

        // Test 3: GET /api/contracts/99999 returns 404
        [Fact] public async Task GetContract_InvalidId_Returns404()
            => Assert.Equal(HttpStatusCode.NotFound,
                (await _client.GetAsync("/api/contracts/99999")).StatusCode);

        // Test 4: Create contract then read it back (data integrity)
        [Fact] public async Task CreateContract_ThenRead_DataIntegrityVerified()
        {
            var token    = await GetTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var clientId   = await CreateClientAsync();
            var contractId = await CreateContractAsync(clientId, "Active");

            var r    = await _client.GetAsync($"/api/contracts/{contractId}");
            Assert.Equal(HttpStatusCode.OK, r.StatusCode);
            var body = JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync(), _json);
            Assert.Equal("Integration Test Contract", body.GetProperty("title").GetString());
        }

        // Test 5: POST with missing fields returns 400
        [Fact] public async Task CreateContract_MissingFields_Returns400()
        {
            var token = await GetTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var r = await _client.PostAsJsonAsync("/api/contracts", new { clientId = 0 });
            Assert.True(r.StatusCode == HttpStatusCode.BadRequest ||
                        r.StatusCode == HttpStatusCode.UnprocessableEntity);
        }

        // Test 6: PATCH /api/contracts/{id}/status updates status correctly
        [Fact] public async Task PatchContractStatus_UpdatesCorrectly()
        {
            var token = await GetTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var clientId   = await CreateClientAsync();
            var contractId = await CreateContractAsync(clientId, "Active");

            var json    = JsonSerializer.Serialize(new { status = "OnHold" });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var req     = new HttpRequestMessage(HttpMethod.Patch, $"/api/contracts/{contractId}/status")
                { Content = content };
            var r = await _client.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, r.StatusCode);

            var get  = await _client.GetAsync($"/api/contracts/{contractId}");
            var body = JsonSerializer.Deserialize<JsonElement>(await get.Content.ReadAsStringAsync(), _json);
            Assert.Equal("OnHold", body.GetProperty("status").GetString());
        }

        // Test 7: GET /api/clients returns 200
        [Fact] public async Task GetClients_Returns200()
            => Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/clients")).StatusCode);

        // Test 8: GET /api/servicerequests returns 200
        [Fact] public async Task GetServiceRequests_Returns200()
            => Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/servicerequests")).StatusCode);

        // Test 9: POST service request on Expired contract returns 422
        [Fact] public async Task CreateServiceRequest_ExpiredContract_Returns422()
        {
            var token = await GetTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var clientId   = await CreateClientAsync();
            var contractId = await CreateContractAsync(clientId, "Expired");

            var r = await _client.PostAsJsonAsync("/api/servicerequests", new
            {
                contractId, description="Test", currency="USD",
                originalCost=1000, exchangeRateToZAR=18.50,
                priority="Normal", requestedBy="Tester"
            });
            Assert.Equal(HttpStatusCode.UnprocessableEntity, r.StatusCode);
        }

        // Test 10: GET /api/currency/rates returns positive rate values
        [Fact] public async Task GetCurrencyRates_ReturnsPositiveRates()
        {
            var r     = await _client.GetAsync("/api/currency/rates");
            Assert.Equal(HttpStatusCode.OK, r.StatusCode);
            var rates = JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync(), _json);
            Assert.True(rates.GetProperty("usdToZar").GetDecimal() > 0);
            Assert.True(rates.GetProperty("eurToZar").GetDecimal() > 0);
            Assert.True(rates.GetProperty("gbpToZar").GetDecimal() > 0);
        }

        // Test 11: DELETE contract returns 204, then 404
        [Fact] public async Task DeleteContract_Returns204_ThenNotFound()
        {
            var token = await GetTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var clientId   = await CreateClientAsync();
            var contractId = await CreateContractAsync(clientId, "Draft");
            Assert.Equal(HttpStatusCode.NoContent,
                (await _client.DeleteAsync($"/api/contracts/{contractId}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound,
                (await _client.GetAsync($"/api/contracts/{contractId}")).StatusCode);
        }

        // Test 12: Login with wrong credentials returns 401
        [Fact] public async Task Login_WrongCredentials_Returns401()
        {
            var r = await _client.PostAsJsonAsync("/api/auth/login",
                new { email="nobody@nowhere.com", password="WrongPassword1" });
            Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
        }

        // HELPERS 
        private async Task<string> GetTokenAsync()
        {
            var email    = $"test_{Guid.NewGuid():N}@glms.com";
            var password = "Test@12345";
            await _client.PostAsJsonAsync("/api/auth/register",
                new { email, password, fullName="Test User" });
            var r    = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
            var body = JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync(), _json);
            return body.GetProperty("token").GetString()!;
        }

        private async Task<int> CreateClientAsync()
        {
            var r = await _client.PostAsJsonAsync("/api/clients", new
            {
                name=$"Test Client {Guid.NewGuid():N}"[..30],
                contactPerson="Tester",
                email=$"t{Guid.NewGuid():N}@t.com",
                phone="+1-555-0001", region="Test"
            });
            var body = JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync(), _json);
            return body.GetProperty("id").GetInt32();
        }

        private async Task<int> CreateContractAsync(int clientId, string status)
        {
            var r = await _client.PostAsJsonAsync("/api/contracts", new
            {
                clientId, title="Integration Test Contract",
                startDate="2024-01-01T00:00:00Z", endDate="2025-01-01T00:00:00Z",
                status, serviceLevel="Standard", contractValueUSD=10000, notes="Test"
            });
            var body = JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync(), _json);
            return body.GetProperty("id").GetInt32();
        }
    }
}
