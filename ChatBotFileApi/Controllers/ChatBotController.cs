using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Threading.Tasks;

namespace ChatbotFileAPI.Controllers
{
    [Route("api/chatbot")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private static string extractedText = "";

        // Upload a File (TXT or PDF)
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                if (file.ContentType == "text/plain") // TXT File
                {
                    using (var reader = new StreamReader(stream))
                    {
                        extractedText = await reader.ReadToEndAsync();
                    }
                }
                else if (file.ContentType == "application/pdf") // PDF File
                {
                    extractedText = ExtractTextFromPdf(stream);
                }
                else
                {
                    return BadRequest("Unsupported file type. Upload a TXT or PDF file.");
                }
            }

            return Ok("File uploaded successfully!");
        }

        //  Chatbot Query Based on Uploaded File
        [HttpPost("ask")]
        public IActionResult AskChatbot([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(extractedText))
                return BadRequest("No file uploaded. Please upload a file first.");

            if (string.IsNullOrWhiteSpace(request.Query))
                return BadRequest("Query cannot be empty.");

            // Simple text search for matching responses
            string response = extractedText.Contains(request.Query, StringComparison.OrdinalIgnoreCase)
                ? "Yes, I found relevant information in the file."
                : "Sorry, I couldn't find relevant information.";

            return Ok(new { Response = response });
        }

       
        private string ExtractTextFromPdf(Stream pdfStream)
        {
            using (PdfReader reader = new PdfReader(pdfStream))
            {
                StringBuilder text = new StringBuilder();
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                }
                return text.ToString();
            }
        }

       
        public class ChatRequest
        {
            public required string Query { get; set; }
        }
    }
}
