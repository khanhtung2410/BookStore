using Bookstore.Books;
using Bookstore.Books.Dto;
using Bookstore.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Web.Controllers
{
    public class ImportController : BookstoreControllerBase
    {
        private readonly IBookImportAppService _bookImportService;
        public ImportController(IBookImportAppService bookImportService)
        {
            _bookImportService = bookImportService;
        }
        [HttpGet]
        public async Task<FileResult> DownloadTemplate()
        {
            var fileBytes = await _bookImportService.DownloadImportTemplateAsync();
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "BookImportTemplate.xlsx");
        }
        [HttpPost("Books/ImportExcel")]
        public async Task<IActionResult> ImportExcel([FromForm] ImportBookExcelDto input)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Jhon");
            if (input.ExcelFile == null || input.ExcelFile.Length == 0)
                return BadRequest("No file uploaded or file is empty.");

            // Manual validation of file extension
            var allowedExtensions = new[] { ".xls", ".xlsx" };
            var extension = Path.GetExtension(input.ExcelFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only .xls or .xlsx files are allowed.");

            try
            {
                using var ms = new MemoryStream();
                await input.ExcelFile.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                await _bookImportService.ImportBooksFromExcel(fileBytes);

                return Ok(new { success = true, message = "Books imported successfully!" });
            }
            catch (Exception ex)
            {
                Logger.Error("Error importing books from Excel", ex);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}

