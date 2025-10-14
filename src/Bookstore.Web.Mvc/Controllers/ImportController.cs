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
        [HttpGet("template")]
        public FileResult DownloadImportTemplate()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Jhon");
            byte[] fileBytes;

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Books");

                //Headers
                string[] headers = {
                "Title", "Author", "Genre", "Description", "Format",
                "Publisher", "PublishedDate (dd-MM-yyyy)", "ISBN",
                "BuyPrice (VND)", "SellPrice", "StockQuantity", "Expenditure (VND)"
                };

                for (int col = 1; col <= headers.Length; col++)
                {
                    sheet.Cells[1, col].Value = headers[col - 1];
                    sheet.Cells[1, col].Style.Font.Bold = true;
                }

                //Table
                var tblRange = sheet.Cells[1, 1, 2, headers.Length]; // Initial 2 rows
                var table = sheet.Tables.Add(tblRange, "BookTable");
                table.ShowHeader = true;

                //Format columns
                sheet.Column(1).Style.Numberformat.Format = "@"; // Title
                sheet.Column(2).Style.Numberformat.Format = "@"; // Author
                sheet.Column(3).Style.Numberformat.Format = "@"; // Genre
                sheet.Column(4).Style.Numberformat.Format = "@"; // Description
                sheet.Column(5).Style.Numberformat.Format = "@"; // Format
                sheet.Column(6).Style.Numberformat.Format = "@"; // Publisher
                sheet.Column(7).Style.Numberformat.Format = "dd-MM-yyyy"; // PublishedDate
                sheet.Column(8).Style.Numberformat.Format = "@"; //ISBN
                sheet.Column(9).Style.Numberformat.Format = "#,##0.00"; // BuyPrice
                sheet.Column(10).Style.Numberformat.Format = "#,##0.00"; // SellPrice
                sheet.Column(11).Style.Numberformat.Format = "0";        // StockQuantity
                sheet.Column(12).Style.Numberformat.Format = "#,##0.00"; // Expenditure

                //Enum validaaation
                //Book Format
                var formatValidation = sheet.DataValidations.AddListValidation("E2:E10");
                formatValidation.ShowErrorMessage = true;
                formatValidation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                formatValidation.ErrorTitle = "Invalid Format";
                formatValidation.Error = "Please select a valid book format.";
                formatValidation.Formula.Values.Add("Paperback");
                formatValidation.Formula.Values.Add("Hardcover");
                sheet.Cells["A1:L1"].Style.Font.Bold = true;
                sheet.Cells.AutoFitColumns();
                //Book Genre
                var genres = new[] {
                "Fiction","NonFiction","Mystery","Thriller","Romance","Fantasy",
                "ScienceFiction","Biography","History","Adventure","Children",
                "SelfHelp","Classic","Travel","Cooking","Horror","GraphicNovel"
            };

                var genreValidation = sheet.DataValidations.AddListValidation("C2:C1000");
                genreValidation.ShowErrorMessage = true;
                genreValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                genreValidation.ErrorTitle = "Invalid Genre";
                genreValidation.Error = "Please select a valid book genre.";
                foreach (var g in genres) genreValidation.Formula.Values.Add(g);

                //Formulas
                for (int row = 2; row <= 1000; row++)
                {
                    sheet.Cells[row, 12].Formula = $"IF(AND(I{row}<>\"\",K{row}<>\"\"),I{row}*K{row},\"\")";
                }


                fileBytes = package.GetAsByteArray();
            }
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "BookImportTemplate.xlsx");
        }
        [HttpPost("Books/ImportExcel")]
        public async Task<IActionResult> ImportExcel([FromForm] ImportBookExcelDto input)
        {
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

