using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Models.ViewModels;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IContractService _contractService;
        private readonly ICurrencyService _currencyService;
        private readonly IServiceRequestFactory _factory;

        public ServiceRequestsController(
            ApplicationDbContext db,
            IContractService contractService,
            ICurrencyService currencyService,
            IServiceRequestFactory factory)
        {
            _db = db;
            _contractService = contractService;
            _currencyService = currencyService;
            _factory = factory;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _db.ServiceRequests
                .Include(r => r.Contract)
                    .ThenInclude(c => c!.Client)
                .OrderByDescending(r => r.DateRaised)
                .ToListAsync();
            return View(requests);
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _db.ServiceRequests
                .Include(r => r.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound();
            return View(request);
        }

        public async Task<IActionResult> Create(int? contractId)
        {
            var rates = await _currencyService.GetRatesAsync();
            var vm = new ServiceRequestFormViewModel
            {
                ContractId   = contractId ?? 0,
                UsdToZar     = rates.UsdToZar,
                EurToZar     = rates.EurToZar,
                GbpToZar     = rates.GbpToZar,
                ContractList = await BuildActiveContractList(contractId)
            };
            ViewBag.Currencies = _currencyService.GetSupportedCurrencies();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestFormViewModel vm)
        {
            // Remove ALL fields that aren't actual form inputs
            ModelState.Remove("ContractList");
            ModelState.Remove("ContractTitle");
            ModelState.Remove("CostZAR");
            ModelState.Remove("ExchangeRateToZAR");
            ModelState.Remove("UsdToZar");
            ModelState.Remove("EurToZar");
            ModelState.Remove("GbpToZar");

            // Server-side fallback: fetch rate if JS didn't set it
            var rates = await _currencyService.GetRatesAsync();
            if (vm.ExchangeRateToZAR <= 0)
                vm.ExchangeRateToZAR = rates.GetRate(vm.Currency);

            // Server-side fallback: calculate ZAR if JS didn't set it
            if (vm.CostZAR <= 0 && vm.OriginalCost > 0)
                vm.CostZAR = Math.Round(vm.OriginalCost * vm.ExchangeRateToZAR, 2);

            // Manual check for ContractId since [Required] on int doesn't work reliably
            if (vm.ContractId <= 0)
                ModelState.AddModelError("ContractId", "Please select a contract.");

            if (!ModelState.IsValid)
            {
                vm.UsdToZar      = rates.UsdToZar;
                vm.EurToZar      = rates.EurToZar;
                vm.GbpToZar      = rates.GbpToZar;
                vm.ContractList  = await BuildActiveContractList(vm.ContractId);
                ViewBag.Currencies = _currencyService.GetSupportedCurrencies();
                return View(vm);
            }

            // Business rule: block Expired / OnHold contracts
            var canRaise = await _contractService.CanRaiseServiceRequestAsync(vm.ContractId);
            if (!canRaise)
            {
                ModelState.AddModelError("ContractId",
                    "Service requests can only be raised against Active or Draft contracts. " +
                    "This contract is Expired or On Hold.");
                vm.UsdToZar      = rates.UsdToZar;
                vm.EurToZar      = rates.EurToZar;
                vm.GbpToZar      = rates.GbpToZar;
                vm.ContractList  = await BuildActiveContractList(vm.ContractId);
                ViewBag.Currencies = _currencyService.GetSupportedCurrencies();
                return View(vm);
            }

            // Factory Pattern — creates ServiceRequest with ZAR conversion
            var request = _factory.Create(
                contractId:   vm.ContractId,
                description:  vm.Description,
                currency:     vm.Currency,
                originalCost: vm.OriginalCost,
                rateToZar:    vm.ExchangeRateToZAR,
                priority:     vm.Priority,
                requestedBy:  vm.RequestedBy
            );

            _db.ServiceRequests.Add(request);
            await _db.SaveChangesAsync();

            TempData["Success"] =
                $"Service request submitted. " +
                $"{vm.Currency} {vm.OriginalCost:N2} = R {request.CostZAR:N2} " +
                $"(rate: {vm.ExchangeRateToZAR:F4})";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var request = await _db.ServiceRequests
                .Include(r => r.Contract)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound();

            var rates = await _currencyService.GetRatesAsync();
            var vm = new ServiceRequestFormViewModel
            {
                Id                = request.Id,
                ContractId        = request.ContractId,
                Description       = request.Description,
                Currency          = request.Currency,
                OriginalCost      = request.OriginalCost,
                CostZAR           = request.CostZAR,
                ExchangeRateToZAR = request.ExchangeRateToZAR,
                Priority          = request.Priority,
                RequestedBy       = request.RequestedBy,
                ContractTitle     = request.Contract?.Title,
                UsdToZar          = rates.UsdToZar,
                EurToZar          = rates.EurToZar,
                GbpToZar          = rates.GbpToZar
            };
            ViewBag.Currencies = _currencyService.GetSupportedCurrencies();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequestFormViewModel vm)
        {
            ModelState.Remove("ContractList");
            ModelState.Remove("ContractTitle");
            ModelState.Remove("CostZAR");
            ModelState.Remove("ExchangeRateToZAR");
            ModelState.Remove("UsdToZar");
            ModelState.Remove("EurToZar");
            ModelState.Remove("GbpToZar");

            var rates = await _currencyService.GetRatesAsync();
            if (vm.ExchangeRateToZAR <= 0)
                vm.ExchangeRateToZAR = rates.GetRate(vm.Currency);

            if (!ModelState.IsValid)
            {
                ViewBag.Currencies = _currencyService.GetSupportedCurrencies();
                return View(vm);
            }

            var request = await _db.ServiceRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Description       = vm.Description;
            request.Currency          = vm.Currency.ToUpper();
            request.OriginalCost      = vm.OriginalCost;
            request.CostZAR           = _currencyService.ConvertToZar(vm.OriginalCost, vm.Currency, rates);
            request.ExchangeRateToZAR = vm.ExchangeRateToZAR;
            request.Priority          = vm.Priority;
            request.RequestedBy       = vm.RequestedBy;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Service request updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var request = await _db.ServiceRequests
                .Include(r => r.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound();
            return View(request);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _db.ServiceRequests.FindAsync(id);
            if (request != null)
            {
                _db.ServiceRequests.Remove(request);
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Service request deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetRates()
        {
            var rates = await _currencyService.GetRatesAsync();
            return Json(new
            {
                usdToZar  = rates.UsdToZar,
                eurToZar  = rates.EurToZar,
                gbpToZar  = rates.GbpToZar,
                isLive    = rates.IsLive,
                fetchedAt = rates.FetchedAt
            });
        }

        private async Task<SelectList> BuildActiveContractList(int? selectedId = null)
        {
            var contracts = await _db.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .OrderBy(c => c.Title)
                .ToListAsync();

            var items = contracts.Select(c => new
            {
                c.Id,
                Display = $"{c.Title} — {c.Client?.Name}"
            });

            return new SelectList(items, "Id", "Display", selectedId);
        }
    }
}