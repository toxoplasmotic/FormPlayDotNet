using iTextSharp.text.pdf;
using FormPlay.Models;
using System.Text;

namespace FormPlay.Services
{
    public class PdfService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PdfService> _logger;

        public PdfService(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<PdfService> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        public string GetPdfTemplatePath()
        {
            var templatePath = _configuration["PdfSettings:TemplatePath"];
            return Path.Combine(_environment.ContentRootPath, templatePath);
        }

        public async Task<Dictionary<string, string>> ExtractPdfFormFieldsAsync(Stream pdfStream)
        {
            var fields = new Dictionary<string, string>();
            
            try
            {
                using (var reader = new PdfReader(pdfStream))
                {
                    AcroFields form = reader.AcroFields;
                    
                    foreach (var field in form.Fields)
                    {
                        string fieldName = field.Key;
                        string fieldValue = form.GetField(fieldName);
                        
                        fields[fieldName] = fieldValue;
                        _logger.LogInformation($"Extracted field: {fieldName} = {fieldValue}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting PDF form fields: {ex.Message}");
                throw;
            }
            
            return fields;
        }

        public async Task<string> SavePdfWithFieldsAsync(TpsReport report, Dictionary<string, string> formFields)
        {
            string savePath = _configuration["PdfSettings:SavePath"];
            string directoryPath = Path.Combine(_environment.WebRootPath, savePath);
            
            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            string fileName = $"TPS_Report_{report.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(directoryPath, fileName);
            
            try
            {
                // Copy the template and fill in the fields
                string templatePath = GetPdfTemplatePath();
                
                using (var reader = new PdfReader(templatePath))
                {
                    using (var stamper = new PdfStamper(reader, new FileStream(filePath, FileMode.Create)))
                    {
                        AcroFields form = stamper.AcroFields;
                        
                        foreach (var field in formFields)
                        {
                            try
                            {
                                form.SetField(field.Key, field.Value);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Could not set field {field.Key}: {ex.Message}");
                                // Continue with other fields
                            }
                        }
                        
                        // Make the form read-only if needed, based on the report status
                        if (report.Status != TpsReportStatus.New)
                        {
                            stamper.FormFlattening = true;
                        }
                    }
                }
                
                // Return the relative path for storing in the database
                return Path.Combine(savePath, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving PDF with fields: {ex.Message}");
                throw;
            }
        }

        public void SetFieldEditability(string pdfPath, List<string> editableFields)
        {
            // In a real implementation, this would modify the PDF to make only certain fields editable
            // This is a complex operation that might require additional PDF libraries
            
            _logger.LogInformation($"Setting editability for PDF {pdfPath} - Editable fields: {string.Join(", ", editableFields)}");
            
            // Placeholder for actual implementation
        }
    }
}
