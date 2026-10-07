using GLMS.API.Models.DTOs;
using GLMS.API.Repositories;
using GLMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.API.Controllers
{
    /// <summary>REST API for Service Request management.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestRepository _repo;
        private readonly IApiContractService _contractSvc;
        private readonly IApiServiceRequestFactory _factory;

        public ServiceRequestsController(IServiceRequestRepository repo,
            IApiContractService contractSvc, IApiServiceRequestFactory factory)
        { _repo=repo; _contractSvc=contractSvc; _factory=factory; }

        /// <summary>Get all service requests.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ServiceRequestDto>), 200)]
        public async Task<IActionResult> GetAll()
            => Ok((await _repo.GetAllAsync()).Select(Map));

        /// <summary>Get service requests for a specific contract.</summary>
        [HttpGet("by-contract/{contractId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ServiceRequestDto>), 200)]
        public async Task<IActionResult> GetByContract(int contractId)
            => Ok((await _repo.GetByContractAsync(contractId)).Select(Map));

        /// <summary>Get a service request by ID.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceRequestDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _repo.GetByIdAsync(id);
            return r is null ? NotFound(new { message = $"Service request {id} not found." }) : Ok(Map(r));
        }

        /// <summary>Create a service request. Blocked if parent contract is Expired or OnHold.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ServiceRequestDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Create([FromBody] CreateServiceRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await _contractSvc.CanRaiseServiceRequestAsync(dto.ContractId))
                return UnprocessableEntity(new { message = "Cannot create request — contract is Expired or OnHold." });
            var sr      = _factory.Create(dto.ContractId, dto.Description, dto.Currency,
                dto.OriginalCost, dto.ExchangeRateToZAR, dto.Priority, dto.RequestedBy);
            var created = await _repo.CreateAsync(sr);
            var full    = await _repo.GetByIdAsync(created.Id);
            return CreatedAtAction(nameof(GetById), new { id=created.Id }, Map(full!));
        }

        /// <summary>Delete a service request.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r is null) return NotFound(new { message = $"Service request {id} not found." });
            await _repo.DeleteAsync(id);
            return NoContent();
        }

        private static ServiceRequestDto Map(GLMS.API.Models.ServiceRequest r) => new()
        {
            Id=r.Id, ContractId=r.ContractId, ContractTitle=r.Contract?.Title ?? "",
            Description=r.Description, Currency=r.Currency, OriginalCost=r.OriginalCost,
            CostZAR=r.CostZAR, ExchangeRateToZAR=r.ExchangeRateToZAR,
            Status=r.Status.ToString(), Priority=r.Priority,
            RequestedBy=r.RequestedBy, DateRaised=r.DateRaised
        };
    }
}
