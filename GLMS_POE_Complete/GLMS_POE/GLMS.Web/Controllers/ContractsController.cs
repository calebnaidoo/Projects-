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
    
    /// Manages freight contracts — CRUD, status changes, PDF uploads, and filtering.
    /// Uses Repository Pattern (IContractRepository) for data access.
    /// Uses Observer Pattern (IContractService) for status change notifications.
    
    [Authorize]
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IContractRepository _repo;
        private readonly IContractService _contractService;
        private readonly IFileService _fileService;

        public ContractsController(
            ApplicationDbContext db,
            IContractRepository repo,
            IContractService contractService,
            IFileService fileService)
        {
            _db = db;
            _repo = repo;
            _contractService = contractService;
            _fileService = fileService;
        }

        /// <summary>
        /// Lists contracts with optional LINQ-based filtering by date, status, and search term.
        /// Uses the Repository Pattern's FilterAsync method.
        /// </summary>
        public async Task<IActionResult> Index(
            DateTime? startFrom, DateTime? startTo,
            ContractStatus? status, string? search)
        {
            var contracts = await _repo.FilterAsync(startFrom, startTo, status, search);
            return View(new ContractFilterViewModel
            {
                StartDateFrom = startFrom,
                StartDateTo = startTo,
                StatusFilter = status,
                SearchTerm = search,
                Contracts = contracts.ToList()
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _repo.GetByIdAsync(id);
            if (contract == null) return NotFound();
            return View(contract);
        }

        public async Task<IActionResult> Create()
        {
            return View(new ContractFormViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                ClientList = await BuildClientList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractFormViewModel vm)
        {
            // Remove fields that aren't required for server-side validation
            ModelState.Remove("SignedAgreement");
            ModelState.Remove("ExistingFileName");
            ModelState.Remove("ClientList");

            if (!ModelState.IsValid)
            {
                vm.ClientList = await BuildClientList();
                return View(vm);
            }

            // Business validation — end date must be after start date
            if (vm.EndDate <= vm.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
                vm.ClientList = await BuildClientList();
                return View(vm);
            }

            var contract = new Contract
            {
                ClientId = vm.ClientId,
                Title = vm.Title,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                Status = vm.Status,
                ServiceLevel = vm.ServiceLevel,
                ContractValueUSD = vm.ContractValueUSD,
                Notes = vm.Notes,
                CreatedAt = DateTime.UtcNow
            };

            // Handle PDF upload if provided
            if (vm.SignedAgreement != null && vm.SignedAgreement.Length > 0)
            {
                if (!_fileService.IsValidPdfFile(vm.SignedAgreement))
                {
                    ModelState.AddModelError("SignedAgreement", "Only PDF files are accepted (max 10 MB).");
                    vm.ClientList = await BuildClientList();
                    return View(vm);
                }
                var (path, name) = await _fileService.SaveAgreementAsync(vm.SignedAgreement);
                contract.SignedAgreementPath = path;
                contract.SignedAgreementFileName = name;
            }

            await _repo.CreateAsync(contract);
            TempData["Success"] = $"Contract '{contract.Title}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c == null) return NotFound();

            return View(new ContractFormViewModel
            {
                Id = c.Id, ClientId = c.ClientId, Title = c.Title,
                StartDate = c.StartDate, EndDate = c.EndDate,
                Status = c.Status, ServiceLevel = c.ServiceLevel,
                ContractValueUSD = c.ContractValueUSD, Notes = c.Notes,
                ExistingFileName = c.SignedAgreementFileName,
                ClientList = await BuildClientList(c.ClientId)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractFormViewModel vm)
        {
            ModelState.Remove("SignedAgreement");
            ModelState.Remove("ExistingFileName");
            ModelState.Remove("ClientList");

            if (!ModelState.IsValid)
            {
                vm.ClientList = await BuildClientList();
                return View(vm);
            }

            var contract = await _repo.GetByIdAsync(id);
            if (contract == null) return NotFound();

            contract.ClientId = vm.ClientId;
            contract.Title = vm.Title;
            contract.StartDate = vm.StartDate;
            contract.EndDate = vm.EndDate;
            contract.Status = vm.Status;
            contract.ServiceLevel = vm.ServiceLevel;
            contract.ContractValueUSD = vm.ContractValueUSD;
            contract.Notes = vm.Notes;

            if (vm.SignedAgreement != null && vm.SignedAgreement.Length > 0)
            {
                if (!_fileService.IsValidPdfFile(vm.SignedAgreement))
                {
                    ModelState.AddModelError("SignedAgreement", "Only PDF files accepted.");
                    vm.ClientList = await BuildClientList();
                    return View(vm);
                }
                _fileService.DeleteAgreement(contract.SignedAgreementPath);
                var (path, name) = await _fileService.SaveAgreementAsync(vm.SignedAgreement);
                contract.SignedAgreementPath = path;
                contract.SignedAgreementFileName = name;
            }

            await _repo.UpdateAsync(contract);
            TempData["Success"] = $"Contract '{contract.Title}' updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c != null)
            {
                _fileService.DeleteAgreement(c.SignedAgreementPath);
                await _repo.DeleteAsync(id);
            }
            TempData["Success"] = "Contract deleted.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Updates contract status and triggers the Observer Pattern notification.
        /// The ContractStatusLogger observer logs the change automatically.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ContractStatus newStatus)
        {
            try
            {
                await _contractService.UpdateStatusAsync(id, newStatus);
                TempData["Success"] = $"Status updated to {newStatus}.";
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Contract not found.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>Serves the uploaded PDF as a file download.</summary>
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c == null || string.IsNullOrEmpty(c.SignedAgreementPath)) return NotFound();

            var fullPath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot",
                c.SignedAgreementPath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath)) return NotFound();

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, "application/pdf", c.SignedAgreementFileName ?? "agreement.pdf");
        }

        // Builds the client dropdown for Create/Edit forms
        private async Task<SelectList> BuildClientList(int? selectedId = null)
        {
            var clients = await _db.Clients.OrderBy(c => c.Name).ToListAsync();
            return new SelectList(clients, "Id", "Name", selectedId);
        }
    }
}
