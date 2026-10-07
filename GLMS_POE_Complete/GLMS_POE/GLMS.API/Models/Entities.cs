using System.ComponentModel.DataAnnotations;

namespace GLMS.API.Models
{
    public enum ContractStatus       { Draft, Active, Expired, OnHold }
    public enum ServiceLevel         { Standard, Express, Premium }
    public enum ServiceRequestStatus { Pending, InProgress, Completed, Cancelled }

    public class Client
    {
        public int    Id            { get; set; }
        [Required][StringLength(200)] public string Name          { get; set; } = string.Empty;
        [Required][StringLength(200)] public string ContactPerson { get; set; } = string.Empty;
        [Required][EmailAddress][StringLength(200)] public string Email { get; set; } = string.Empty;
        [Required][Phone][StringLength(50)]         public string Phone  { get; set; } = string.Empty;
        [Required][StringLength(100)]               public string Region { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }

    public class Contract
    {
        public int    Id              { get; set; }
        [Required] public int ClientId { get; set; }
        [Required][StringLength(300)] public string Title { get; set; } = string.Empty;
        [Required] public DateTime StartDate              { get; set; }
        [Required] public DateTime EndDate                { get; set; }
        public ContractStatus Status                      { get; set; } = ContractStatus.Draft;
        public ServiceLevel   ServiceLevel                { get; set; } = ServiceLevel.Standard;
        [Range(0, double.MaxValue)] public decimal ContractValueUSD { get; set; }
        public string? SignedAgreementPath     { get; set; }
        public string? SignedAgreementFileName { get; set; }
        [StringLength(1000)] public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Client?  Client   { get; set; }
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }

    public class ServiceRequest
    {
        public int    Id          { get; set; }
        [Required] public int ContractId  { get; set; }
        [Required][StringLength(1000)] public string Description { get; set; } = string.Empty;
        [Required][StringLength(3)]    public string Currency    { get; set; } = "USD";
        [Range(0.01, double.MaxValue)] public decimal OriginalCost      { get; set; }
        public decimal CostZAR           { get; set; }
        public decimal ExchangeRateToZAR { get; set; }
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
        [StringLength(50)] public string Priority    { get; set; } = "Normal";
        [Required][StringLength(200)] public string RequestedBy { get; set; } = string.Empty;
        public DateTime DateRaised { get; set; } = DateTime.UtcNow;
        public Contract? Contract  { get; set; }
    }
}
