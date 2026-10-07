using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    /// <summary>
    /// Full CRUD operations for logistics clients.
    /// All actions require authentication.
    /// </summary>
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ClientsController(ApplicationDbContext db) => _db = db;

        /// <summary>Lists all clients with their contract counts.</summary>
        public async Task<IActionResult> Index() =>
            View(await _db.Clients.Include(c => c.Contracts)
                .OrderBy(c => c.Name).ToListAsync());

        /// <summary>Shows full client details including all linked contracts.</summary>
        public async Task<IActionResult> Details(int id)
        {
            var client = await _db.Clients
                .Include(c => c.Contracts)
                    .ThenInclude(co => co.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null) return NotFound();
            return View(client);
        }

        public IActionResult Create() => View(new Client());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid) return View(client);

            client.CreatedAt = DateTime.UtcNow;
            _db.Clients.Add(client);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Client '{client.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _db.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return BadRequest();
            if (!ModelState.IsValid) return View(client);

            _db.Clients.Update(client);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Client '{client.Name}' updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = await _db.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _db.Clients.FindAsync(id);
            if (client != null)
            {
                _db.Clients.Remove(client);
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Client deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
