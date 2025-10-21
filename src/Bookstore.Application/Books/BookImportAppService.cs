using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI.Inputs;
using Bookstore.Books.Dto;
using Bookstore.Entities.Books;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Books
{
    public class BookImportAppService : ApplicationService, IBookImportAppService
    {
        private readonly IRepository<Book, int> _bookRepository;
        private readonly IRepository<BookInventory, int> _bookInventoryRepository;
        private readonly IRepository<BookEdition, int> _bookEditionRepository;

        public BookImportAppService(
            IRepository<Book, int> bookRepository,
            IRepository<BookInventory, int> bookInventoryRepository,
            IRepository<BookEdition, int> bookEditionRepository)
        {
            _bookRepository = bookRepository;
            _bookInventoryRepository = bookInventoryRepository;
            _bookEditionRepository = bookEditionRepository;
        }

        public async Task<byte[]> DownloadImportTemplateAsync()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Jhon");

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Books");

            // Headers
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

            // Table for 1000 rows
            var tblRange = sheet.Cells[1, 1, 1000, headers.Length];
            var table = sheet.Tables.Add(tblRange, "BookTable");
            table.ShowHeader = true;

            // Column formatting
            sheet.Column(1).Style.Numberformat.Format = "@"; // Title
            sheet.Column(2).Style.Numberformat.Format = "@"; // Author
            sheet.Column(3).Style.Numberformat.Format = "@"; // Genre
            sheet.Column(4).Style.Numberformat.Format = "@"; // Description
            sheet.Column(5).Style.Numberformat.Format = "@"; // Format
            sheet.Column(6).Style.Numberformat.Format = "@"; // Publisher
            sheet.Column(7).Style.Numberformat.Format = "dd-MM-yyyy"; // PublishedDate
            sheet.Column(8).Style.Numberformat.Format = "@"; // ISBN
            sheet.Column(9).Style.Numberformat.Format = "#,##0.00"; // BuyPrice
            sheet.Column(10).Style.Numberformat.Format = "#,##0.00"; // SellPrice
            sheet.Column(11).Style.Numberformat.Format = "0";        // StockQuantity
            sheet.Column(12).Style.Numberformat.Format = "#,##0.00"; // Expenditure

            // Format validation
            var formatValidation = sheet.DataValidations.AddListValidation("E2:E1000");
            formatValidation.Formula.Values.Add("Paperback");
            formatValidation.Formula.Values.Add("Hardcover");
            formatValidation.ErrorTitle = "Invalid Format";
            formatValidation.Error = "Please select a valid book format.";
            formatValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;

            // Genre validation
            var genres = new[] {
            "Fiction","NonFiction","Mystery","Thriller","Romance","Fantasy",
            "ScienceFiction","Biography","History","Adventure","Children",
            "SelfHelp","Classic","Travel","Cooking","Horror","GraphicNovel"
        };
            var genreValidation = sheet.DataValidations.AddListValidation("C2:C1000");
            foreach (var g in genres) genreValidation.Formula.Values.Add(g);
            genreValidation.ErrorTitle = "Invalid Genre";
            genreValidation.Error = "Please select a valid book genre.";
            genreValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;

            // Expenditure formula
            for (int row = 2; row <= 1000; row++)
            {
                sheet.Cells[row, 12].Formula = $"IF(AND(ISNUMBER(I{row}),ISNUMBER(K{row})),I{row}*K{row},0)";
            }

            sheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public async Task ImportBooksFromExcel(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                throw new ArgumentException("File is empty.");

            using (var stream = new MemoryStream(fileBytes))
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new ArgumentException("No worksheet found in the Excel file.");

                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var title = worksheet.Cells[row, 1].Text;
                        var author = worksheet.Cells[row, 2].Text;
                        var genreText = worksheet.Cells[row, 3].Text;
                        var description = worksheet.Cells[row, 4].Text;
                        var formatText = worksheet.Cells[row, 5].Text;
                        var publisher = worksheet.Cells[row, 6].Text;
                        var publishedDateText = worksheet.Cells[row, 7].Text;
                        var isbn = worksheet.Cells[row, 8].Text;
                        var buyPriceText = worksheet.Cells[row, 9].Text;
                        var sellPriceText = worksheet.Cells[row, 10].Text;
                        var stockQuantityText = worksheet.Cells[row, 11].Text;

                        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
                            throw new ArgumentException($"Missing required fields in row {row}.");

                        if (!Enum.TryParse<BookConsts.Genre>(genreText, out var genre))
                            throw new ArgumentException($"Invalid genre '{genreText}' in row {row}.");

                        if (!Enum.TryParse<BookConsts.Format>(formatText, out var format))
                            throw new ArgumentException($"Invalid format '{formatText}' in row {row}.");

                        if (!DateTime.TryParse(publishedDateText, out var publishedDate))
                            throw new ArgumentException($"Invalid published date '{publishedDateText}' in row {row}.");

                        if (!decimal.TryParse(buyPriceText, out var buyPrice))
                            throw new ArgumentException($"Invalid buy price '{buyPriceText}' in row {row}.");

                        if (!decimal.TryParse(sellPriceText, out var sellPrice))
                            throw new ArgumentException($"Invalid sell price '{sellPriceText}' in row {row}.");

                        if (!int.TryParse(stockQuantityText, out var stockQuantity))
                            throw new ArgumentException($"Invalid stock quantity '{stockQuantityText}' in row {row}.");

                        // Check if the book exists
                        var existingBook = await _bookRepository.FirstOrDefaultAsync(b => b.Title == title && b.Author == author);
                        if (existingBook != null)
                        {
                            var existingEdition = await _bookEditionRepository.FirstOrDefaultAsync(e => e.ISBN == isbn);
                            if (existingEdition != null)
                            {
                                var inventory = await _bookInventoryRepository.FirstOrDefaultAsync(i => i.BookEditionId == existingEdition.Id);
                                if (inventory != null)
                                {
                                    inventory.StockQuantity += stockQuantity;
                                    await _bookInventoryRepository.UpdateAsync(inventory);
                                    continue;
                                }
                            }
                            else
                            {
                                // Add new edition + inventory
                                var newEdition = new BookEdition
                                {
                                    BookId = existingBook.Id,
                                    ISBN = isbn,
                                    Format = format,
                                    Publisher = publisher,
                                    PublishedDate = publishedDate,
                                };
                                var editionId = await _bookEditionRepository.InsertAndGetIdAsync(newEdition);

                                var newInventory = new BookInventory
                                {
                                    BuyPrice = buyPrice,
                                    SellPrice = sellPrice,
                                    BookEditionId = editionId,
                                    StockQuantity = stockQuantity
                                };
                                await _bookInventoryRepository.InsertAsync(newInventory);

                                continue;
                            }
                        }

                        // Add new book, edition, and inventory
                        var book = new Book
                        {
                            Title = title,
                            Author = author,
                            Genre = genre,
                            Description = description
                        };
                        var bookId = await _bookRepository.InsertAndGetIdAsync(book);

                        var edition = new BookEdition
                        {
                            BookId = bookId,
                            ISBN = isbn,
                            Format = format,
                            Publisher = publisher,
                            PublishedDate = publishedDate,
                        };
                        var editionIdNew = await _bookEditionRepository.InsertAndGetIdAsync(edition);

                        var inventoryNew = new BookInventory
                        {
                            BuyPrice = buyPrice,
                            SellPrice = sellPrice,
                            BookEditionId = editionIdNew,
                            StockQuantity = stockQuantity
                        };
                        await _bookInventoryRepository.InsertAsync(inventoryNew);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error processing row {row}: {ex.Message}");
                    }
                }
            }
        }
    }
}
