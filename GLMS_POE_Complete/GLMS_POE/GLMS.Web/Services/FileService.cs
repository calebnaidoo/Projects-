namespace GLMS.Web.Services
{
    // ─────────────────────────────────────────────────────────────────────────
    // FILE SERVICE — handles PDF upload, validation and deletion.
    // Only PDF files with correct extension AND content-type are accepted.
    // Files are saved with a GUID prefix to prevent name collisions.
    // ─────────────────────────────────────────────────────────────────────────

    public interface IFileService
    {
        /// <summary>Saves a validated PDF to the server and returns its path and original name.</summary>
        Task<(string path, string fileName)> SaveAgreementAsync(IFormFile file);

        /// <summary>Deletes a previously uploaded file from the server.</summary>
        void DeleteAgreement(string? filePath);

        /// <summary>Returns true only if the file is a valid PDF with correct content type.</summary>
        bool IsValidPdfFile(IFormFile? file);

        /// <summary>Returns the file extension (e.g. ".pdf", ".exe").</summary>
        string GetFileExtension(IFormFile file);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;

        // Only PDF files are accepted
        private static readonly string[] AllowedExtensions = { ".pdf" };
        private static readonly string[] AllowedContentTypes = { "application/pdf" };

        // Maximum file size: 10 MB
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Saves the uploaded PDF to /wwwroot/uploads/agreements/ using a UUID prefix
        /// to prevent filename collisions between different contracts.
        /// </summary>
        public async Task<(string path, string fileName)> SaveAgreementAsync(IFormFile file)
        {
            if (!IsValidPdfFile(file))
                throw new InvalidOperationException("Only PDF files up to 10 MB are accepted.");

            // Ensure the uploads directory exists
            var folder = Path.Combine(_env.WebRootPath, "uploads", "agreements");
            Directory.CreateDirectory(folder);

            // UUID prefix prevents overwriting files with same original name
            var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(folder, uniqueName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            _logger.LogInformation("Agreement saved: {FileName}", uniqueName);

            // Return relative web path (for storing in DB) and original name (for display)
            return ($"/uploads/agreements/{uniqueName}", file.FileName);
        }

        /// <summary>Deletes the physical file from disk if it exists.</summary>
        public void DeleteAgreement(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var full = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(full))
            {
                File.Delete(full);
                _logger.LogInformation("Agreement deleted: {Path}", full);
            }
        }

        /// <summary>
        /// Validates the file by checking BOTH extension AND content-type.
        /// This prevents attackers from renaming a .exe to .pdf to bypass validation.
        /// </summary>
        public bool IsValidPdfFile(IFormFile? file)
        {
            if (file == null || file.Length == 0) return false;

            // Check extension
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext)) return false;

            // Check content-type (prevents spoofing with renamed files)
            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant())) return false;

            // Check file size
            if (file.Length > MaxFileSizeBytes) return false;

            return true;
        }

        /// <summary>Returns the file extension including the dot (e.g. ".pdf").</summary>
        public string GetFileExtension(IFormFile file)
            => Path.GetExtension(file.FileName);
    }
}
