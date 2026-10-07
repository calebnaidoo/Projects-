using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Models.ViewModels
{
  
    // ACCOUNT VIEW MODELS
    

    /// <summary>View model for the registration page.</summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(200)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>View model for the login page.</summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

   
    // DASHBOARD VIEW MODEL
    

    /// <summary>Aggregates stats shown on the main dashboard.</summary>
    public class DashboardViewModel
    {
        public int TotalClients { get; set; }
        public int TotalContracts { get; set; }
        public int ActiveContracts { get; set; }
        public int ExpiredContracts { get; set; }
        public int TotalServiceRequests { get; set; }
        public int PendingRequests { get; set; }

        // Live exchange rates fetched from the API
        public decimal UsdToZar { get; set; }
        public decimal EurToZar { get; set; }
        public decimal GbpToZar { get; set; }

        public List<Contract> RecentContracts { get; set; } = new();
        public List<ServiceRequest> RecentRequests { get; set; } = new();
    }

    
    // CONTRACT VIEW MODELS
    
    /// <summary>Used for both Create and Edit contract forms.</summary>
    public class ContractFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a client.")]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Contract title is required.")]
        [StringLength(300)]
        [Display(Name = "Contract Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required.")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required.")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);

        [Display(Name = "Status")]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Display(Name = "Service Level")]
        public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;

        [Display(Name = "Contract Value (USD)")]
        [Range(0, double.MaxValue)]
        public decimal ContractValueUSD { get; set; }

        [Display(Name = "Signed Agreement (PDF only)")]
        public IFormFile? SignedAgreement { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Passed back to view to show existing file name
        public string? ExistingFileName { get; set; }

        // Populated in controller for the client dropdown
        public SelectList? ClientList { get; set; }
    }

    /// <summary>Filter parameters for the contracts list page.</summary>
    public class ContractFilterViewModel
    {
        [DataType(DataType.Date)]
        public DateTime? StartDateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDateTo { get; set; }

        public ContractStatus? StatusFilter { get; set; }
        public string? SearchTerm { get; set; }
        public List<Contract> Contracts { get; set; } = new();
    }

    
    // SERVICE REQUEST VIEW MODELS
   

    /// <summary>
    /// Used for the Create service request form.
    /// Supports multi-currency input (USD, EUR, GBP).
    /// </summary>
    public class ServiceRequestFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a contract.")]
        [Display(Name = "Contract")]
        public int ContractId { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a currency.")]
        [Display(Name = "Currency")]
        public string Currency { get; set; } = "USD";

        [Required(ErrorMessage = "Cost is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than zero.")]
        [Display(Name = "Cost")]
        public decimal OriginalCost { get; set; }

        // Calculated server-side after form submission
        [Display(Name = "Cost (ZAR)")]
        public decimal CostZAR { get; set; }

        [Display(Name = "Exchange Rate to ZAR")]
        public decimal ExchangeRateToZAR { get; set; }

        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Normal";

        [Required(ErrorMessage = "Requested by is required.")]
        [StringLength(200)]
        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; } = string.Empty;

        // Populated in controller — only Active contracts shown
        public SelectList? ContractList { get; set; }

        // Live exchange rates passed to the view for JavaScript converter
        public decimal UsdToZar { get; set; }
        public decimal EurToZar { get; set; }
        public decimal GbpToZar { get; set; }

        public string? ContractTitle { get; set; }
    }
}
