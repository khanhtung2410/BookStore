using Abp.Application.Services;
using Abp.Domain.Repositories;
using Bookstore.Books.Dto;
using Bookstore.Entities;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
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

        public Task ImportBookHandle(UpdateBookDto input)
        {
            throw new NotImplementedException();
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
                            var existingEdition = await _bookEditionRepository.FirstOrDefaultAsync(e => e.BookId == existingBook.Id && e.ISBN == isbn);
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
