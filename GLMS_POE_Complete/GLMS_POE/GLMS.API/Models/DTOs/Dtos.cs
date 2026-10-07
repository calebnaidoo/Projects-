using System.ComponentModel.DataAnnotations;
using GLMS.API.Models;

namespace GLMS.API.Models.DTOs
{
    public class LoginDto
    {
        [Required][EmailAddress] public string Email    { get; set; } = string.Empty;
        [Required]               public string Password { get; set; } = string.Empty;
    }
    public class RegisterDto
    {
        [Required][EmailAddress] public string Email    { get; set; } = string.Empty;
        [Required][MinLength(6)] public string Password { get; set; } = string.Empty;
        [Required]               public string FullName { get; set; } = string.Empty;
    }
    public class AuthResponseDto
    {
        public string Token   { get; set; } = string.Empty;
        public string Email   { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }
    public class ClientDto
    {
        public int Id { get; set; } public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty; public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty; public string Region { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } public int ContractCount { get; set; }
    }
    public class CreateClientDto
    {
        [Required][StringLength(200)] public string Name          { get; set; } = string.Empty;
        [Required][StringLength(200)] public string ContactPerson { get; set; } = string.Empty;
        [Required][EmailAddress]      public string Email         { get; set; } = string.Empty;
        [Required][Phone]             public string Phone         { get; set; } = string.Empty;
        [Required][StringLength(100)] public string Region        { get; set; } = string.Empty;
    }
    public class ContractDto
    {
        public int Id { get; set; } public int ClientId { get; set; } public string ClientName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty; public DateTime StartDate { get; set; } public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty; public string ServiceLevel { get; set; } = string.Empty;
        public decimal ContractValueUSD { get; set; } public string? Notes { get; set; }
        public string? SignedAgreementFileName { get; set; } public bool HasAgreement { get; set; }
        public DateTime CreatedAt { get; set; } public int ServiceRequestCount { get; set; }
    }
    public class CreateContractDto
    {
        [Required] public int ClientId { get; set; }
        [Required][StringLength(300)] public string Title { get; set; } = string.Empty;
        [Required] public DateTime StartDate { get; set; } [Required] public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; } = ContractStatus.Draft;
        public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;
        [Range(0, double.MaxValue)] public decimal ContractValueUSD { get; set; }
        [StringLength(1000)] public string? Notes { get; set; }
    }
    public class UpdateContractDto
    {
        [Required][StringLength(300)] public string Title { get; set; } = string.Empty;
        [Required] public DateTime StartDate { get; set; } [Required] public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; } public ServiceLevel ServiceLevel { get; set; }
        [Range(0, double.MaxValue)] public decimal ContractValueUSD { get; set; }
        [StringLength(1000)] public string? Notes { get; set; }
    }
    public class PatchStatusDto { [Required] public ContractStatus Status { get; set; } }
    public class ServiceRequestDto
    {
        public int Id { get; set; } public int ContractId { get; set; } public string ContractTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; public string Currency { get; set; } = string.Empty;
        public decimal OriginalCost { get; set; } public decimal CostZAR { get; set; }
        public decimal ExchangeRateToZAR { get; set; } public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; public string RequestedBy { get; set; } = string.Empty;
        public DateTime DateRaised { get; set; }
    }
    public class CreateServiceRequestDto
    {
        [Required] public int ContractId { get; set; }
        [Required][StringLength(1000)] public string Description { get; set; } = string.Empty;
        [Required][StringLength(3)] public string Currency { get; set; } = "USD";
        [Range(0.01, double.MaxValue)] public decimal OriginalCost { get; set; }
        public decimal ExchangeRateToZAR { get; set; }
        [StringLength(50)] public string Priority { get; set; } = "Normal";
        [Required][StringLength(200)] public string RequestedBy { get; set; } = string.Empty;
    }
    public class CurrencyRatesDto
    {
        public decimal UsdToZar { get; set; } public decimal EurToZar { get; set; }
        public decimal GbpToZar { get; set; } public bool IsLive { get; set; } public DateTime FetchedAt { get; set; }
    }
}
