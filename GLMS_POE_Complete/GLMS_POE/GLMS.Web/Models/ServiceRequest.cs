using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    /// <summary>
    /// Tracks the lifecycle of a logistics service request.
    /// </summary>
    public enum ServiceRequestStatus { Pending, InProgress, Completed, Cancelled }

    /// <summary>
    /// Represents a logistics service request raised against a contract.
    /// Stores cost in both the original currency AND ZAR for local reporting.
    /// Business Rule: Cannot be raised against Expired or OnHold contracts.
    /// </summary>
    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>The currency the cost was entered in (USD, EUR, GBP, etc.)</summary>
        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(3)]
        [Display(Name = "Currency")]
        public string Currency { get; set; } = "USD";

        /// <summary>Original cost in the selected currency.</summary>
        [Required(ErrorMessage = "Cost is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than zero.")]
        [Display(Name = "Cost (Original Currency)")]
        public decimal OriginalCost { get; set; }

        /// <summary>Cost converted to ZAR for local financial reporting.</summary>
        [Display(Name = "Cost (ZAR)")]
        public decimal CostZAR { get; set; }

        /// <summary>The exchange rate to ZAR used at time of creation.</summary>
        [Display(Name = "Exchange Rate to ZAR")]
        public decimal ExchangeRateToZAR { get; set; }

        [Display(Name = "Status")]
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        [Display(Name = "Priority")]
        [StringLength(50)]
        public string Priority { get; set; } = "Normal";

        [Required(ErrorMessage = "Requested by is required.")]
        [StringLength(200)]
        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; } = string.Empty;

        [Display(Name = "Date Raised")]
        public DateTime DateRaised { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Contract? Contract { get; set; }
    }
}
