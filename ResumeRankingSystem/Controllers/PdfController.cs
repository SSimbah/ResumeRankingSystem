using iText.Kernel.Pdf;
using iText.Forms;
using iText.Forms.Fields;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace YourApp.Controllers
{
    public class PdfController : Controller
    {
        // GET: Pdf/Index
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            // Path to your template file
            string filePath = Path.Combine("wwwroot", "images", "resume_template.pdf");
            string fileName = "ResumeTemplate.pdf";

            // Return the file as a download
            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/pdf", fileName);
            }

            return NotFound("Template not found.");
        }

        // POST: Pdf/ExtractText
        [HttpPost]
        public IActionResult ExtractText(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "No file selected.";
                return View("Index");
            }

            string extractedText = ExtractTextFromPdf(file);
            var parsedFields = ParseExtractedText(extractedText);

            // Pass the parsed fields to the view for confirmation
            ViewBag.ParsedFields = parsedFields;

            // Return the view where the user can confirm the data
            return View("FormResults");
        }


        private string ExtractTextFromPdf(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                using (var pdfReader = new PdfReader(stream))
                {
                    using (var pdfDocument = new PdfDocument(pdfReader))
                    {
                        if (pdfDocument == null)
                        {
                            return "PDF document could not be opened.";
                        }

                        var form = PdfAcroForm.GetAcroForm(pdfDocument, false);
                        if (form == null)
                        {
                            return "No form found in the PDF.";
                        }

                        string extractedText = string.Empty;
                        var processedFields = new HashSet<string>();

                        // Iterate over all the fields in the form
                        var fields = form.GetFormFields();
                        foreach (var field in fields)
                        {
                            var fieldName = field.Key; // Field name
                            var fieldValue = field.Value.GetValueAsString(); // Field value

                            // Normalize the field name to ignore numeric suffixes after a dot (e.g., "name_input.1")
                            string normalizedFieldName = NormalizeFieldName(fieldName);

                            // Only process each unique field name once
                            if (!processedFields.Contains(normalizedFieldName))
                            {
                                extractedText += $"{fieldName}:\n{fieldValue}\n";
                                processedFields.Add(normalizedFieldName); // Add normalized name to set to prevent duplicates
                            }
                        }

                        return extractedText;
                    }
                }
            }
        }

        // Normalize field name to remove numeric suffixes like "name_input.1"
        private string NormalizeFieldName(string fieldName)
        {
            return Regex.Replace(fieldName, @"\.\d+$", "");
        }

        // Parse the extracted text and map it to the field names
        private Dictionary<string, string> ParseExtractedText(string extractedText)
        {
            // Define the field names you expect
            var fieldNames = new List<string>
            {
                "fname_input",
                "mname_input",
                "lname_input",
                "objective_input",
                "email_input",
                "phone_input",
                "address_input",
                "gender_input",
                "age_input",
                "experience_input",
                "education_input",
                "skills_input"
            };

            var parsedFields = new Dictionary<string, string>();

            // Split the extracted text into lines
            var lines = extractedText.Split(new[] { '\n' }, StringSplitOptions.None);

            string currentFieldName = null;
            StringBuilder currentFieldValue = new StringBuilder();

            foreach (var line in lines)
            {
                // Check if the line starts with one of the field names
                bool isFieldLine = false;
                foreach (var fieldName in fieldNames)
                {
                    if (line.StartsWith(fieldName))
                    {
                        // If we already have a previous field, save it
                        if (currentFieldName != null)
                        {
                            parsedFields[currentFieldName] = currentFieldValue.ToString().Trim();
                        }

                        // Set the current field name and reset the field value builder
                        currentFieldName = fieldName;
                        currentFieldValue.Clear();

                        // Capture the value after the field name and the colon, without the colon
                        var value = line.Substring(fieldName.Length).TrimStart(':').Trim();
                        currentFieldValue.Append(value);
                        isFieldLine = true;
                        break;
                    }
                }

                // If the line is not a new field, append the line to the current field's value
                if (!isFieldLine && currentFieldName != null)
                {
                    currentFieldValue.AppendLine(line.Trim());
                }
            }

            // After the loop, add the last field value
            if (currentFieldName != null)
            {
                parsedFields[currentFieldName] = currentFieldValue.ToString().Trim();
            }

            return parsedFields;
        }

    }
}
