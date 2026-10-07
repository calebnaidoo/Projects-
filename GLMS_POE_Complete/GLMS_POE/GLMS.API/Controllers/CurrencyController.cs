using GLMS.API.Models.DTOs;
using GLMS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.API.Controllers
{
    /// <summary>Exposes live exchange rates (USD/EUR/GBP to ZAR).</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CurrencyController : ControllerBase
    {
        private readonly IApiCurrencyService _currency;
        public CurrencyController(IApiCurrencyService currency) => _currency = currency;

        /// <summary>Get current USD, EUR, GBP to ZAR rates.</summary>
        [HttpGet("rates")]
        [ProducesResponseType(typeof(CurrencyRatesDto), 200)]
        public async Task<IActionResult> GetRates()
        {
            var rates = await _currency.GetRatesAsync();
            return Ok(new CurrencyRatesDto
            {
                UsdToZar=rates.UsdToZar, EurToZar=rates.EurToZar,
                GbpToZar=rates.GbpToZar, IsLive=rates.IsLive, FetchedAt=rates.FetchedAt
            });
        }
    }
}
