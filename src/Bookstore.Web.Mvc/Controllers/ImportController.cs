using Bookstore.Controllers;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.IO;
using System.Threading.Tasks;

namespace Bookstore.Web.Controllers
{
    public class ImportController : BookstoreControllerBase
    {
        [HttpGet("template")]
        public FileResult DownloadImportTemplate()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Jhon");
            byte[] fileBytes;

            using (var package = new ExcelPackage(new FileInfo("BookImport")))
            {
                var sheet = package.Workbook.Worksheets.Add("Books");
                var tblRange = sheet.Cells["A1:L100"];
                var table = sheet.Tables.Add(tblRange, "BookTable");

                //Headers
                sheet.Cells["A1"].Value = "Title";
                sheet.Cells["B1"].Value = "Author";
                sheet.Cells["C1"].Value = "Genre";
                sheet.Cells["D1"].Value = "Description";
                sheet.Cells["E1"].Value = "Format";
                sheet.Cells["F1"].Value = "Publisher";
                sheet.Cells["G1"].Value = "PublishedDate (dd-MM-yyyy)";
                sheet.Cells["H1"].Value = "ISBN";
                sheet.Cells["I1"].Value = "BuyPrice (VND)";
                sheet.Cells["J1"].Value = "SellPrice";
                sheet.Cells["K1"].Value = "StockQuantity";
                sheet.Cells["L1"].Value = "Expenditure (VND)";

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
                var formatValidation = sheet.DataValidations.AddListValidation("E2:E100");
                formatValidation.ShowErrorMessage = true;
                formatValidation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                formatValidation.ErrorTitle = "Invalid Format";
                formatValidation.Error = "Please select a valid book format.";
                formatValidation.Formula.Values.Add("Paperback");
                formatValidation.Formula.Values.Add("Hardcover");
                sheet.Cells["A1:L1"].Style.Font.Bold = true;
                sheet.Cells.AutoFitColumns();
                //Book Genre
                var genreValidation = sheet.DataValidations.AddListValidation("C2:C100");
                genreValidation.ShowErrorMessage = true;
                genreValidation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                genreValidation.ErrorTitle = "Invalid Format";
                genreValidation.Error = "Please select a valid book genre.";
                genreValidation.Formula.Values.Add("Fiction");
                genreValidation.Formula.Values.Add("NonFiction");
                genreValidation.Formula.Values.Add("Mystery");
                genreValidation.Formula.Values.Add("Thriller");
                genreValidation.Formula.Values.Add("Romance");
                genreValidation.Formula.Values.Add("Fantasy");
                genreValidation.Formula.Values.Add("ScienceFiction");
                genreValidation.Formula.Values.Add("Biography");
                genreValidation.Formula.Values.Add("History");
                genreValidation.Formula.Values.Add("Adventure");
                genreValidation.Formula.Values.Add("Children");
                genreValidation.Formula.Values.Add("SelfHelp");
                genreValidation.Formula.Values.Add("Classic");
                genreValidation.Formula.Values.Add("Travel");
                genreValidation.Formula.Values.Add("Cooking");
                genreValidation.Formula.Values.Add("Horror");
                genreValidation.Formula.Values.Add("GraphicNovel");

                //Formulas
                sheet.Cells["L2:L100"].Formula = "I2*K2";


                fileBytes = package.GetAsByteArray();
            }
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "BookImportTemplate.xlsx");
        }
        public Task<IActionResult> ReadData()
        {
            return Task.FromResult<IActionResult>(View());
        }
    }
}
