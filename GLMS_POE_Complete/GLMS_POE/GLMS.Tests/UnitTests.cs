using GLMS.API.Data;
using GLMS.API.Models;
using GLMS.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GLMS.Tests
{
    // ══════════════════════════════════════════════════════════════════════════
    // PART 2 UNIT TESTS — Currency Calculation (Tests 1–10)
    // These are the same tests from Part 2 AllTests.cs, now testing the API
    // layer's CurrencyService which has the same logic
    // ══════════════════════════════════════════════════════════════════════════
    public class CurrencyServiceTests
    {
        private readonly ApiCurrencyService _sut;
        private static readonly ApiCurrencyRates TestRates = new()
        { UsdToZar=18.50m, EurToZar=20.10m, GbpToZar=23.40m, IsLive=false };

        public CurrencyServiceTests()
        {
            var http   = new HttpClient { BaseAddress = new Uri("https://localhost:9999/") };
            var logger = new Mock<ILogger<ApiCurrencyService>>().Object;
            var config = new Mock<IConfiguration>().Object;
            _sut = new ApiCurrencyService(http, logger, config);
        }

        [Fact] public void ConvertToZar_100USD_Returns1850()
            => Assert.Equal(1850.00m, _sut.ConvertToZar(100m, "USD", TestRates));

        [Fact] public void ConvertToZar_100EUR_Returns2010()
            => Assert.Equal(2010.00m, _sut.ConvertToZar(100m, "EUR", TestRates));

        [Fact] public void ConvertToZar_100GBP_Returns2340()
            => Assert.Equal(2340.00m, _sut.ConvertToZar(100m, "GBP", TestRates));

        [Fact] public void ConvertToZar_LargeAmount_CorrectResult()
            => Assert.Equal(4_625_000.00m, _sut.ConvertToZar(250_000m, "USD", TestRates));

        [Fact] public void ConvertToZar_ZeroAmount_ReturnsZero()
            => Assert.Equal(0.00m, _sut.ConvertToZar(0m, "USD", TestRates));

        [Fact] public void ConvertToZar_Result_HasMaxTwoDecimalPlaces()
        {
            var rates  = new ApiCurrencyRates { UsdToZar = 18.333333m };
            var result = _sut.ConvertToZar(1m, "USD", rates);
            var dp     = BitConverter.GetBytes(decimal.GetBits(result)[3])[2];
            Assert.True(dp <= 2);
        }

        [Fact] public void ConvertToZar_NegativeAmount_ThrowsArgumentException()
            => Assert.Throws<ArgumentException>(() => _sut.ConvertToZar(-100m, "USD", TestRates));

        [Fact] public void ConvertToZar_ZeroRate_ThrowsArgumentException()
            => Assert.Throws<ArgumentException>(() =>
                _sut.ConvertToZar(100m, "USD", new ApiCurrencyRates { UsdToZar = 0m }));

        [Fact] public void CurrencyRates_GetRate_USD_ReturnsUsdToZar()
            => Assert.Equal(18.50m, TestRates.GetRate("USD"));

        [Theory]
        [InlineData(100,   "USD", 18.50,   1850.00)]
        [InlineData(200,   "EUR", 20.10,   4020.00)]
        [InlineData(500,   "GBP", 23.40,  11700.00)]
        [InlineData(75.50, "USD", 18.50,   1396.75)]
        [InlineData(8750,  "USD", 18.50, 161875.00)]
        public void ConvertToZar_Parameterised_AllCorrect(
            decimal amount, string currency, decimal rate, decimal expected)
        {
            var rates = new ApiCurrencyRates { UsdToZar=rate, EurToZar=rate, GbpToZar=rate };
            Assert.Equal(expected, _sut.ConvertToZar(amount, currency, rates));
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // PART 2 UNIT TESTS — File Validation (Tests 11–20)
    // ══════════════════════════════════════════════════════════════════════════
    public class FileValidationTests
    {
        private static readonly string[] AllowedExtensions   = { ".pdf" };
        private static readonly string[] AllowedContentTypes = { "application/pdf" };
        private const long MaxBytes = 10 * 1024 * 1024; // 10MB

        private static bool IsValidPdf(IFormFile? file)
        {
            if (file == null || file.Length == 0) return false;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext)) return false;
            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant())) return false;
            if (file.Length > MaxBytes) return false;
            return true;
        }

        private static IFormFile MockFile(string name, string contentType, long size = 1024)
        {
            var f = new Mock<IFormFile>();
            f.Setup(x => x.FileName).Returns(name);
            f.Setup(x => x.ContentType).Returns(contentType);
            f.Setup(x => x.Length).Returns(size);
            return f.Object;
        }

        [Fact] public void IsValidPdf_ValidPdf_ReturnsTrue()
            => Assert.True(IsValidPdf(MockFile("agreement.pdf", "application/pdf", 500_000)));

        [Fact] public void IsValidPdf_ExeFile_ReturnsFalse()
            => Assert.False(IsValidPdf(MockFile("malware.exe", "application/octet-stream")));

        [Fact] public void IsValidPdf_DocxFile_ReturnsFalse()
            => Assert.False(IsValidPdf(MockFile("doc.docx",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document")));

        [Fact] public void IsValidPdf_JpgFile_ReturnsFalse()
            => Assert.False(IsValidPdf(MockFile("photo.jpg", "image/jpeg")));

        [Fact] public void IsValidPdf_ZipFile_ReturnsFalse()
            => Assert.False(IsValidPdf(MockFile("archive.zip", "application/zip")));

        [Fact] public void IsValidPdf_SpoofedExtensionWrongContentType_ReturnsFalse()
            => Assert.False(IsValidPdf(MockFile("fake.pdf", "application/octet-stream")));

        [Fact] public void IsValidPdf_NullFile_ReturnsFalse()
            => Assert.False(IsValidPdf(null));

        [Fact] public void IsValidPdf_EmptyFile_ReturnsFalse()
            => Assert.False(IsValidPdf(MockFile("empty.pdf", "application/pdf", 0)));

        [Fact] public void GetExtension_PdfFile_ReturnsDotPdf()
            => Assert.Equal(".pdf", Path.GetExtension("agreement.pdf"));

        [Fact] public void GetExtension_ExeFile_ReturnsDotExe()
            => Assert.Equal(".exe", Path.GetExtension("virus.exe"));
    }

    // ══════════════════════════════════════════════════════════════════════════
    // PART 2 UNIT TESTS — ServiceRequest Factory (Tests 21–25)
    // ══════════════════════════════════════════════════════════════════════════
    public class ServiceRequestFactoryTests
    {
        private readonly ApiServiceRequestFactory _factory = new();

        [Fact] public void Create_ValidInputs_ReturnsCorrectRequest()
        {
            var r = _factory.Create(1, "Emergency reroute", "USD", 1000m, 18.50m, "High", "James");
            Assert.NotNull(r);
            Assert.Equal(18500.00m, r.CostZAR);
            Assert.Equal("USD", r.Currency);
            Assert.Equal(ServiceRequestStatus.Pending, r.Status);
        }

        [Fact] public void Create_NewRequest_StatusIsPending()
            => Assert.Equal(ServiceRequestStatus.Pending,
                _factory.Create(1, "Test", "USD", 100m, 18.50m, "Normal", "Tester").Status);

        [Fact] public void Create_UsdAmount_ZarCalculatedCorrectly()
        {
            var r = _factory.Create(1, "Test", "USD", 12500m, 18.50m, "Normal", "Tester");
            Assert.Equal(231250.00m, r.CostZAR);
        }

        [Fact] public void Create_ZeroCost_ThrowsArgumentException()
            => Assert.Throws<ArgumentException>(() =>
                _factory.Create(1, "Test", "USD", 0m, 18.50m, "Normal", "Tester"));

        [Fact] public void Create_NegativeCost_ThrowsArgumentException()
            => Assert.Throws<ArgumentException>(() =>
                _factory.Create(1, "Test", "USD", -500m, 18.50m, "Normal", "Tester"));
    }

    // ══════════════════════════════════════════════════════════════════════════
    // PART 2 UNIT TESTS — Contract Business Rules (Tests 26–29)
    // ══════════════════════════════════════════════════════════════════════════
    public class ContractBusinessRuleTests
    {
        private static DbContextOptions<ApiDbContext> Options() =>
            new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        private static async Task SeedContract(ApiDbContext db, int id, ContractStatus status)
        {
            db.Clients.Add(new Client { Id=99, Name="Test", ContactPerson="T",
                Email="t@t.com", Phone="000", Region="T" });
            db.Contracts.Add(new Contract { Id=id, ClientId=99, Title="Test Contract",
                StartDate=DateTime.Today, EndDate=DateTime.Today.AddYears(1), Status=status });
            await db.SaveChangesAsync();
        }

        [Fact] public async Task CanRaiseRequest_ActiveContract_ReturnsTrue()
        {
            await using var db = new ApiDbContext(Options());
            await SeedContract(db, 1, ContractStatus.Active);
            Assert.True(await new ApiContractService(db).CanRaiseServiceRequestAsync(1));
        }

        [Fact] public async Task CanRaiseRequest_ExpiredContract_ReturnsFalse()
        {
            await using var db = new ApiDbContext(Options());
            await SeedContract(db, 1, ContractStatus.Expired);
            Assert.False(await new ApiContractService(db).CanRaiseServiceRequestAsync(1));
        }

        [Fact] public async Task CanRaiseRequest_OnHoldContract_ReturnsFalse()
        {
            await using var db = new ApiDbContext(Options());
            await SeedContract(db, 1, ContractStatus.OnHold);
            Assert.False(await new ApiContractService(db).CanRaiseServiceRequestAsync(1));
        }

        [Fact] public async Task UpdateStatus_DraftToActive_PersistedCorrectly()
        {
            await using var db = new ApiDbContext(Options());
            await SeedContract(db, 1, ContractStatus.Draft);
            await new ApiContractService(db).UpdateStatusAsync(1, ContractStatus.Active);
            Assert.Equal(ContractStatus.Active, (await db.Contracts.FindAsync(1))!.Status);
        }
    }
}
