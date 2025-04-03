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

        public string GetPdfTemplatePath(string templateName = null)
        {
            // If no template name is specified, use the default template
            if (string.IsNullOrEmpty(templateName))
            {
                templateName = _configuration["PdfSettings:DefaultTemplate"];
            }
            
            // Get the template path from configuration
            var templatePath = _configuration[$"PdfSettings:Templates:{templateName}:Path"];
            
            if (string.IsNullOrEmpty(templatePath))
            {
                _logger.LogWarning($"Template path not found for template '{templateName}'. Falling back to vanilla template.");
                templatePath = _configuration["PdfSettings:Templates:vanilla:Path"];
            }
            
            return Path.Combine(_environment.ContentRootPath, templatePath);
        }
        
        public List<TemplateInfo> GetAvailableTemplates()
        {
            var templates = new List<TemplateInfo>();
            var templatesSection = _configuration.GetSection("PdfSettings:Templates");
            
            if (templatesSection != null)
            {
                foreach (var child in templatesSection.GetChildren())
                {
                    var name = child.Key;
                    var displayName = child["Name"] ?? name;
                    var description = child["Description"] ?? "";
                    var path = child["Path"] ?? "";
                    
                    templates.Add(new TemplateInfo
                    {
                        Name = name,
                        DisplayName = displayName,
                        Description = description,
                        Path = path
                    });
                }
            }
            
            return templates;
        }
        
        public class TemplateInfo
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string Path { get; set; }
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

        public async Task<string> SavePdfWithFieldsAsync(TpsReport report, Dictionary<string, string> formFields, string templateName = null)
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
                // Use the specified template or try to get it from the report's TemplateType field if it exists
                string template = templateName;
                if (string.IsNullOrEmpty(template) && !string.IsNullOrEmpty(report.TemplateType))
                {
                    template = report.TemplateType;
                }
                
                // Get the template path (uses default if none specified)
                string templatePath = GetPdfTemplatePath(template);
                
                _logger.LogInformation($"Using template '{template ?? "default"}' at path: {templatePath}");
                
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
