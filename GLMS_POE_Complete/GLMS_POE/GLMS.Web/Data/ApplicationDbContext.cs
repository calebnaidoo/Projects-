using GLMS.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Data
{
    /// <summary>
    /// Main EF Core database context.
    /// Extends IdentityDbContext to include ASP.NET Core Identity tables
    /// (AspNetUsers, AspNetRoles, etc.) alongside application tables.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Application entity sets
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Must call base to configure Identity tables
            base.OnModelCreating(builder);

            // CLIENT configuration 
            builder.Entity<Client>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.ContactPerson).IsRequired().HasMaxLength(200);
                e.Property(x => x.Email).IsRequired().HasMaxLength(200);
                e.Property(x => x.Phone).IsRequired().HasMaxLength(50);
                e.Property(x => x.Region).IsRequired().HasMaxLength(100);
            });

            // CONTRACT configuration 
            builder.Entity<Contract>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).IsRequired().HasMaxLength(300);
                e.Property(x => x.ContractValueUSD).HasColumnType("decimal(18,2)");
                // Store enum as string for readability in SSMS
                e.Property(x => x.Status).HasConversion<string>();
                e.Property(x => x.ServiceLevel).HasConversion<string>();
                // One Client → Many Contracts
                e.HasOne(x => x.Client)
                 .WithMany(x => x.Contracts)
                 .HasForeignKey(x => x.ClientId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // SERVICE REQUEST configuration 
            builder.Entity<ServiceRequest>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Description).IsRequired().HasMaxLength(1000);
                e.Property(x => x.Currency).IsRequired().HasMaxLength(3);
                e.Property(x => x.OriginalCost).HasColumnType("decimal(18,2)");
                e.Property(x => x.CostZAR).HasColumnType("decimal(18,2)");
                e.Property(x => x.ExchangeRateToZAR).HasColumnType("decimal(18,4)");
                e.Property(x => x.Status).HasConversion<string>();
                // One Contract → Many ServiceRequests (cascade delete)
                e.HasOne(x => x.Contract)
                 .WithMany(x => x.ServiceRequests)
                 .HasForeignKey(x => x.ContractId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // SEED DATA 
            builder.Entity<Client>().HasData(
                new Client
                {
                    Id = 1, Name = "Global Freight Partners",
                    ContactPerson = "James Thornton", Email = "james@gfp.com",
                    Phone = "+1-555-0101", Region = "North America",
                    CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
                },
                new Client
                {
                    Id = 2, Name = "EuroShip Logistics GmbH",
                    ContactPerson = "Helga Müller", Email = "helga@euroship.de",
                    Phone = "+49-30-5551", Region = "Europe",
                    CreatedAt = new DateTime(2024, 2, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new Client
                {
                    Id = 3, Name = "AsiaPac Cargo Ltd",
                    ContactPerson = "Li Wei", Email = "li.wei@asiapac.hk",
                    Phone = "+852-2555-0303", Region = "Asia-Pacific",
                    CreatedAt = new DateTime(2024, 3, 5, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            builder.Entity<Contract>().HasData(
                new Contract
                {
                    Id = 1, ClientId = 1,
                    Title = "Trans-Atlantic Freight Agreement 2024",
                    StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Status = ContractStatus.Active, ServiceLevel = ServiceLevel.Premium,
                    ContractValueUSD = 250000m, Notes = "Annual renewal with priority SLA",
                    CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
                },
                new Contract
                {
                    Id = 2, ClientId = 2,
                    Title = "European Distribution Contract Q1",
                    StartDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2024, 6, 30, 0, 0, 0, DateTimeKind.Utc),
                    Status = ContractStatus.Expired, ServiceLevel = ServiceLevel.Standard,
                    ContractValueUSD = 85000m, Notes = "Q1 seasonal contract",
                    CreatedAt = new DateTime(2024, 2, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new Contract
                {
                    Id = 3, ClientId = 3,
                    Title = "Asia-Pacific Express Lane SLA",
                    StartDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    Status = ContractStatus.Active, ServiceLevel = ServiceLevel.Express,
                    ContractValueUSD = 175000m, Notes = "Express delivery for electronics",
                    CreatedAt = new DateTime(2024, 3, 5, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            builder.Entity<ServiceRequest>().HasData(
                new ServiceRequest
                {
                    Id = 1, ContractId = 1,
                    Description = "Emergency container reroute - Hurricane Dorian impact",
                    Currency = "USD", OriginalCost = 12500m,
                    CostZAR = 231250m, ExchangeRateToZAR = 18.50m,
                    Status = ServiceRequestStatus.Completed, Priority = "High",
                    RequestedBy = "James Thornton",
                    DateRaised = new DateTime(2024, 4, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServiceRequest
                {
                    Id = 2, ContractId = 3,
                    Description = "Priority shipment - Medical equipment to Singapore",
                    Currency = "USD", OriginalCost = 8750m,
                    CostZAR = 161875m, ExchangeRateToZAR = 18.50m,
                    Status = ServiceRequestStatus.InProgress, Priority = "Urgent",
                    RequestedBy = "Li Wei",
                    DateRaised = new DateTime(2024, 5, 20, 0, 0, 0, DateTimeKind.Utc)
                },
                new ServiceRequest
                {
                    Id = 3, ContractId = 1,
                    Description = "Customs clearance documentation - Rotterdam port",
                    Currency = "EUR", OriginalCost = 3200m,
                    CostZAR = 62400m, ExchangeRateToZAR = 19.50m,
                    Status = ServiceRequestStatus.Pending, Priority = "Normal",
                    RequestedBy = "James Thornton",
                    DateRaised = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
