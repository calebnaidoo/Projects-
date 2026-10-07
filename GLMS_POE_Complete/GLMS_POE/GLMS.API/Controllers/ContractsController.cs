using GLMS.API.Models;
using GLMS.API.Models.DTOs;
using GLMS.API.Repositories;
using GLMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.API.Controllers
{
    /// <summary>REST API for Contract management — full CRUD plus status patch.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class ContractsController : ControllerBase
    {
        private readonly IContractRepository _repo;
        private readonly IApiContractService _svc;
        private readonly ContractStatusLogger _logger;

        public ContractsController(IContractRepository repo, IApiContractService svc, ContractStatusLogger logger)
        {
            _repo   = repo;
            _svc    = svc;
            _logger = logger;
            _svc.RegisterObserver(_logger);
        }

        /// <summary>Get all contracts with optional filtering by date, status, and search term.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ContractDto>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? startFrom, [FromQuery] DateTime? startTo,
            [FromQuery] ContractStatus? status, [FromQuery] string? search)
        {
            var contracts = await _repo.GetAllAsync(startFrom, startTo, status, search);
            return Ok(contracts.Select(Map));
        }

        /// <summary>Get a single contract by ID.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ContractDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            return c is null ? NotFound(new { message = $"Contract {id} not found." }) : Ok(Map(c));
        }

        /// <summary>Create a new contract.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ContractDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateContractDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.EndDate <= dto.StartDate)
                return BadRequest(new { message = "End date must be after start date." });
            var c = new Contract
            {
                ClientId=dto.ClientId, Title=dto.Title, StartDate=dto.StartDate, EndDate=dto.EndDate,
                Status=dto.Status, ServiceLevel=dto.ServiceLevel,
                ContractValueUSD=dto.ContractValueUSD, Notes=dto.Notes, CreatedAt=DateTime.UtcNow
            };
            var created = await _repo.CreateAsync(c);
            var full    = await _repo.GetByIdAsync(created.Id);
            return CreatedAtAction(nameof(GetById), new { id=created.Id }, Map(full!));
        }

        /// <summary>Update a contract (all fields).</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ContractDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateContractDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var c = await _repo.GetByIdAsync(id);
            if (c is null) return NotFound(new { message = $"Contract {id} not found." });
            c.Title=dto.Title; c.StartDate=dto.StartDate; c.EndDate=dto.EndDate;
            c.Status=dto.Status; c.ServiceLevel=dto.ServiceLevel;
            c.ContractValueUSD=dto.ContractValueUSD; c.Notes=dto.Notes;
            await _repo.UpdateAsync(c);
            return Ok(Map((await _repo.GetByIdAsync(id))!));
        }

        /// <summary>Update only the status of a contract (approve/decline/expire).</summary>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(typeof(ContractDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PatchStatus(int id, [FromBody] PatchStatusDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _svc.UpdateStatusAsync(id, dto.Status);
                return Ok(Map((await _repo.GetByIdAsync(id))!));
            }
            catch (KeyNotFoundException) { return NotFound(new { message = $"Contract {id} not found." }); }
        }

        /// <summary>Delete a contract and all its service requests.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c is null) return NotFound(new { message = $"Contract {id} not found." });
            await _repo.DeleteAsync(id);
            return NoContent();
        }

        private static ContractDto Map(Contract c) => new()
        {
            Id=c.Id, ClientId=c.ClientId, ClientName=c.Client?.Name ?? "",
            Title=c.Title, StartDate=c.StartDate, EndDate=c.EndDate,
            Status=c.Status.ToString(), ServiceLevel=c.ServiceLevel.ToString(),
            ContractValueUSD=c.ContractValueUSD, Notes=c.Notes,
            SignedAgreementFileName=c.SignedAgreementFileName,
            HasAgreement=!string.IsNullOrEmpty(c.SignedAgreementPath),
            CreatedAt=c.CreatedAt, ServiceRequestCount=c.ServiceRequests.Count
        };
    }
}
