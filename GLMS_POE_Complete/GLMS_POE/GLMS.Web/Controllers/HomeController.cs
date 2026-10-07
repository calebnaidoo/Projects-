using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Models.ViewModels;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    /// <summary>
    /// Dashboard controller — shows system-wide statistics and live exchange rates.
    /// All pages require authentication ([Authorize]).
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrencyService _currency;

        public HomeController(ApplicationDbContext db, ICurrencyService currency)
        {
            _db = db;
            _currency = currency;
        }

        
        /// Main dashboard — aggregates counts and fetches live exchange rates
        /// to display on the stats panel.
        
        public async Task<IActionResult> Index()
        {
            // Fetch live rates for dashboard display (USD, EUR, GBP → ZAR)
            var rates = await _currency.GetRatesAsync();

            var vm = new DashboardViewModel
            {
                TotalClients = await _db.Clients.CountAsync(),
                TotalContracts = await _db.Contracts.CountAsync(),
                ActiveContracts = await _db.Contracts.CountAsync(c => c.Status == ContractStatus.Active),
                ExpiredContracts = await _db.Contracts.CountAsync(c => c.Status == ContractStatus.Expired),
                TotalServiceRequests = await _db.ServiceRequests.CountAsync(),
                PendingRequests = await _db.ServiceRequests.CountAsync(s => s.Status == ServiceRequestStatus.Pending),
                UsdToZar = rates.UsdToZar,
                EurToZar = rates.EurToZar,
                GbpToZar = rates.GbpToZar,
                RecentContracts = await _db.Contracts
                    .Include(c => c.Client)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5).ToListAsync(),
                RecentRequests = await _db.ServiceRequests
                    .Include(r => r.Contract).ThenInclude(c => c!.Client)
                    .OrderByDescending(r => r.DateRaised)
                    .Take(5).ToListAsync()
            };

            return View(vm);
        }
    }
}
