using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    /// <summary>
    /// Contract status lifecycle — Draft → Active → Expired / OnHold.
    /// Service requests can only be raised on Active or Draft contracts.
    /// </summary>
    public enum ContractStatus { Draft, Active, Expired, OnHold }

    /// <summary>
    /// Service level determines priority of freight handling.
    /// </summary>
    public enum ServiceLevel { Standard, Express, Premium }

    /// <summary>
    /// Represents a legal freight agreement between TechMove and a client.
    /// One contract can have many service requests (one-to-many relationship).
    /// </summary>
    public class Contract
    {
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Contract title is required.")]
        [StringLength(300)]
        [Display(Name = "Contract Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required.")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Status")]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Display(Name = "Service Level")]
        public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;

        [Display(Name = "Contract Value (USD)")]
        [Range(0, double.MaxValue, ErrorMessage = "Value must be positive.")]
        public decimal ContractValueUSD { get; set; }

        // Path to uploaded PDF on the server
        public string? SignedAgreementPath { get; set; }

        // Original filename shown to user
        public string? SignedAgreementFileName { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Client? Client { get; set; }
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}
