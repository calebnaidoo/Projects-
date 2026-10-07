using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    /// <summary>
    /// Represents a TechMove logistics client organisation.
    /// One client can have many contracts (one-to-many relationship).
    /// </summary>
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client name is required.")]
        [StringLength(200)]
        [Display(Name = "Client Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person is required.")]
        [StringLength(200)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(200)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(50)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Region is required.")]
        [StringLength(100)]
        [Display(Name = "Region")]
        public string Region { get; set; } = string.Empty;

        [Display(Name = "Registered On")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property — one client has many contracts
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
