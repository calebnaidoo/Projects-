using GLMS.API.Models;
using GLMS.API.Models.DTOs;
using GLMS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.API.Controllers
{
    /// <summary>REST API for Client management.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly IClientRepository _repo;
        public ClientsController(IClientRepository repo) => _repo = repo;

        /// <summary>Get all clients.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ClientDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var clients = await _repo.GetAllAsync();
            return Ok(clients.Select(c => new ClientDto
            {
                Id=c.Id, Name=c.Name, ContactPerson=c.ContactPerson,
                Email=c.Email, Phone=c.Phone, Region=c.Region,
                CreatedAt=c.CreatedAt, ContractCount=c.Contracts.Count
            }));
        }

        /// <summary>Get a client by ID.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ClientDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c is null) return NotFound(new { message = $"Client {id} not found." });
            return Ok(new ClientDto { Id=c.Id, Name=c.Name, ContactPerson=c.ContactPerson,
                Email=c.Email, Phone=c.Phone, Region=c.Region, CreatedAt=c.CreatedAt, ContractCount=c.Contracts.Count });
        }

        /// <summary>Create a new client.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ClientDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateClientDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var client = new Client { Name=dto.Name, ContactPerson=dto.ContactPerson,
                Email=dto.Email, Phone=dto.Phone, Region=dto.Region, CreatedAt=DateTime.UtcNow };
            var created = await _repo.CreateAsync(client);
            return CreatedAtAction(nameof(GetById), new { id=created.Id },
                new ClientDto { Id=created.Id, Name=created.Name, ContactPerson=created.ContactPerson,
                    Email=created.Email, Phone=created.Phone, Region=created.Region, CreatedAt=created.CreatedAt });
        }

        /// <summary>Update a client.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ClientDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateClientDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var client = await _repo.GetByIdAsync(id);
            if (client is null) return NotFound(new { message = $"Client {id} not found." });
            client.Name=dto.Name; client.ContactPerson=dto.ContactPerson;
            client.Email=dto.Email; client.Phone=dto.Phone; client.Region=dto.Region;
            await _repo.UpdateAsync(client);
            return Ok(new ClientDto { Id=client.Id, Name=client.Name, ContactPerson=client.ContactPerson,
                Email=client.Email, Phone=client.Phone, Region=client.Region, CreatedAt=client.CreatedAt });
        }

        /// <summary>Delete a client.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c is null) return NotFound(new { message = $"Client {id} not found." });
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
