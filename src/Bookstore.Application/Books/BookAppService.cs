using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Localization;
using Abp.UI;
using Bookstore.Books.Dto;
using Bookstore.Entities.Books;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Books
{
    public class BookAppService : ApplicationService, IBookAppService
    {
        private readonly IRepository<Book, int> _bookRepository;
        private readonly IRepository<BookInventory, int> _bookInventoryRepository;
        private readonly IRepository<BookEdition, int> _bookEditionRepository;
        private readonly IRepository<BookImage, int> _bookImageRepository;
        private readonly IWebHostEnvironment _env;
        private readonly ILocalizationManager _localizationManager;

        public BookAppService(
            IRepository<Book, int> bookRepository, 
            IRepository<BookInventory, int> bookInventoryRepository, 
            IRepository<BookEdition, int> bookEditionRepository, 
            IRepository<BookImage, int> bookImageRepository,
            IWebHostEnvironment env,
            ILocalizationManager localizationManager)
        {
            _bookRepository = bookRepository;
            _bookInventoryRepository = bookInventoryRepository;
            _bookEditionRepository = bookEditionRepository;
            _bookImageRepository = bookImageRepository;
            _env = env;
            _localizationManager = localizationManager;

            LocalizationSourceName = BookstoreConsts.LocalizationSourceName;
        }
        [Abp.Authorization.AbpAuthorize("Pages.Books.Create")]
        public async Task<int> CreateBook(CreateBookDto input)
        {
            if (input.Editions == null || !input.Editions.Any())
                throw new UserFriendlyException(L("BookMustHaveAtLeastOneEdition"));
            if (string.IsNullOrWhiteSpace(input.Title))
            {
                throw new UserFriendlyException(L("TitleIsRequired"));
            }
            if (input.Title.Length > 250)
            {
                throw new UserFriendlyException(L("TitleMaxLengthExceeded", L("Title"), 250));
            }

            if (string.IsNullOrWhiteSpace(input.Author))
            {
                throw new UserFriendlyException(L("AuthorIsRequired"));
            }
            if (input.Author.Length > 200)
            {
                throw new UserFriendlyException(L("AuthorMaxLengthExceeded", L("Author"), 200));
            }

            if (string.IsNullOrWhiteSpace(input.Description))
            {
                throw new UserFriendlyException(L("DescriptionIsRequired"));
            }
            if (input.Description.Length > 1000)
            {
                throw new UserFriendlyException(L("DescriptionMaxLengthExceeded", L("Description"), 1000));
            }

            if (!Enum.IsDefined(typeof(BookConsts.Genre), input.Genre))
            {
                throw new UserFriendlyException(L("InvalidGenre"));
            }

            if (input.Editions == null || !input.Editions.Any())
            {
                throw new UserFriendlyException(L("BookMustHaveAtLeastOneEdition"));
            }

            var existingBook = await _bookRepository
                .FirstOrDefaultAsync(b => b.Title == input.Title && b.Author == input.Author);

            int createdBookId;

            if (existingBook != null)
            {
                createdBookId = existingBook.Id;
            }
            else
            {
                var newBook = new Book(
                    input.Title,
                    input.Author,
                    input.Genre,
                    input.Description
                );
                createdBookId = await _bookRepository.InsertAndGetIdAsync(newBook);
            }

            foreach (var editionDto in input.Editions)
            {
                if (!Enum.IsDefined(typeof(BookConsts.Format), editionDto.Format))
                {
                    throw new UserFriendlyException(L("InvalidFormat"));
                }
                if (string.IsNullOrWhiteSpace(editionDto.ISBN))
                {
                    throw new UserFriendlyException(L("ISBNIsRequired"));
                }

                if (!editionDto.ISBN.All(char.IsDigit))
                {
                    throw new UserFriendlyException(L("InvalidISBNFormat", editionDto.ISBN));
                }
                if (editionDto.ISBN.Length != 10 && editionDto.ISBN.Length != 13)
                {
                    throw new UserFriendlyException(L("InvalidISBNLength", editionDto.ISBN));
                }

                var existingEdition = await _bookEditionRepository.FirstOrDefaultAsync(e => e.ISBN == editionDto.ISBN);
                if (existingEdition != null)
                {
                    throw new UserFriendlyException(L("DuplicateISBN", editionDto.ISBN));
                }

                if (editionDto.PublishedDate == null)
                {
                    throw new UserFriendlyException(L("PublishedDateIsRequired"));
                }
                if (editionDto.PublishedDate > DateTime.Now.AddYears(1))
                {
                    throw new UserFriendlyException(L("InvalidPublishedDateFuture"));
                }
                if (string.IsNullOrWhiteSpace(editionDto.Publisher))
                {
                    throw new UserFriendlyException(L("PublisherIsRequired"));
                }
                if (editionDto.Publisher.Length > 200)
                {
                    throw new UserFriendlyException(L("PublisherMaxLengthExceeded", L("Publisher"), 200));
                }
                var edition = new BookEdition(
                    createdBookId,
                    editionDto.Format,
                    editionDto.Publisher,
                    editionDto.PublishedDate ?? DateTime.Now,
                    editionDto.ISBN
                );
                var editionId = await _bookEditionRepository.InsertAndGetIdAsync(edition);

                // For each Edition, create its Inventory if exist
                if (editionDto.Inventory != null)
                {
                    if (editionDto.Inventory.BuyPrice < 0)
                    {
                        throw new UserFriendlyException(L("BuyPriceNonNegative"));
                    }
                    if (editionDto.Inventory.SellPrice < 0)
                    {
                        throw new UserFriendlyException(L("SellPriceNonNegative"));
                    }
                    if (editionDto.Inventory.StockQuantity < 0)
                    {
                        throw new UserFriendlyException(L("StockQuantityNonNegative"));
                    }
                    var inventory = new BookInventory(
                        editionId,
                        editionDto.Inventory.BuyPrice,
                        editionDto.Inventory.SellPrice,
                        editionDto.Inventory.StockQuantity
                    );

                    await _bookInventoryRepository.InsertAsync(inventory);
                }
            }

            return createdBookId;
        }
        [Abp.Authorization.AbpAuthorize("Pages.Books.Delete")]
        public async Task DeleteBook(DeleteBookDto input)
        {
            await _bookRepository.DeleteAsync(input.Id);
        }

        public async Task<PagedResultDto<ListBookDto>> GetAllBooks(GetAllBooksInput input = null)
        {
            input ??= new GetAllBooksInput();

            var query = _bookRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(input.Keyword))
            {
                query = query.Where(b => b.Title.Contains(input.Keyword) || b.Author.Contains(input.Keyword));
            }

            if (input.Genre.HasValue)
            {
                query = query.Where(b => b.Genre == input.Genre.Value);
            }


            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.Title)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var mapped = ObjectMapper.Map<List<ListBookDto>>(items);

            return new PagedResultDto<ListBookDto>(totalCount, mapped);
        }
        public async Task<BookDto> GetBook(int id)
        {
            var book = await _bookRepository.GetAllIncluding(b => b.Editions).FirstOrDefaultAsync(b => b.Id == id);

            if (book == null || book.IsDeleted)
                return null;
            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                Genre = book.Genre,
                Editions = new List<BookEditionDto>()
            };
            foreach (var edition in book.Editions)
            {
                var inventory = await _bookInventoryRepository.FirstOrDefaultAsync(bi => bi.BookEditionId == edition.Id);
                bookDto.Editions.Add(new BookEditionDto
                {
                    Id = edition.Id,
                    BookId = edition.BookId,
                    Format = edition.Format,
                    Publisher = edition.Publisher,
                    PublishedDate = edition.PublishedDate,
                    ISBN = edition.ISBN,
                    Inventory = inventory != null ? new BookInventoryDto
                    {
                        Id = inventory.Id,
                        BookEditionId = inventory.BookEditionId,
                        BuyPrice = inventory.BuyPrice,
                        SellPrice = inventory.SellPrice,
                        StockQuantity = inventory.StockQuantity
                    } : null,
                    Discount = null
                });
            }
            return bookDto;
        }
        public async Task<BookDto> GetBookEditionByIdAsync(int bookId, int bookEditionId)
        {
            var book = await _bookRepository
                .GetAllIncluding(b => b.Editions).Include(b => b.Editions)
                .ThenInclude(e => e.Inventory)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                throw new UserFriendlyException(L("BookNotFound"));

            // Find the specific edition
            var edition = book.Editions.FirstOrDefault(e => e.Id == bookEditionId);
            if (edition == null)
                throw new UserFriendlyException(L("BookEditionNotFound"));

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                Genre = book.Genre,
                Editions = new List<BookEditionDto>
        {
            new BookEditionDto
            {
                Id = edition.Id,
                Format = edition.Format,
                ISBN = edition.ISBN,
                PublishedDate = edition.PublishedDate,
                Publisher = edition.Publisher,
                Inventory = edition.Inventory != null
                    ? new BookInventoryDto
                    {
                        Id = edition.Inventory.Id,
                        StockQuantity = edition.Inventory.StockQuantity,
                        BuyPrice = edition.Inventory.BuyPrice,
                        SellPrice = edition.Inventory.SellPrice
                    }
                    : null
            }
        }
            };

            return bookDto;
        }

        [Abp.Authorization.AbpAuthorize("Pages.Books.Update")]
        public async Task UpdateBook(UpdateBookDto input)
        {
            var book = await _bookRepository
                .GetAllIncluding(b => b.Editions)
                .FirstOrDefaultAsync(b => b.Id == input.Id);

            if (book == null)
                throw new UserFriendlyException(L("BookNotFound"));
            if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Length > 250)
                throw new UserFriendlyException(L("TitleIsRequiredOrTooLong"));

            if (string.IsNullOrWhiteSpace(input.Author) || input.Author.Length > 200)
                throw new UserFriendlyException(L("AuthorIsRequiredOrTooLong"));

            if (!string.IsNullOrEmpty(input.Description) && input.Description.Length > 1000)
                throw new UserFriendlyException(L("DescriptionTooLong"));

            if (!Enum.IsDefined(typeof(BookConsts.Genre), input.Genre))
                throw new UserFriendlyException(L("InvalidGenre"));

            // Update main book fields
            book.Title = input.Title;
            book.Author = input.Author;
            book.Description = input.Description;
            book.Genre = input.Genre;

            var incomingEditionIds = input.Editions?.Where(e => e.Id > 0).Select(e => e.Id).ToList() ?? new List<int>();

            // Delete removed editions
            var deletedEditions = book.Editions
                .Where(e => !incomingEditionIds.Contains(e.Id))
                .ToList();

            foreach (var del in deletedEditions)
            {
                await _bookEditionRepository.DeleteAsync(del);
                book.Editions.Remove(del);
            }

            // Add or update editions (without inventory)
            foreach (var editionInput in input.Editions)
            {
                BookEdition edition;

                if (editionInput.Id > 0)
                {
                    // Existing edition
                    edition = await _bookEditionRepository
                        .FirstOrDefaultAsync(e => e.Id == editionInput.Id);

                    if (edition == null)
                        continue;

                    var duplicate = await _bookEditionRepository
                        .FirstOrDefaultAsync(e => e.ISBN == editionInput.ISBN && e.Id != editionInput.Id);

                    if (duplicate != null)
                    {
                        throw new UserFriendlyException(L("DuplicateISBN", editionInput.ISBN));
                    }

                    if (string.IsNullOrWhiteSpace(editionInput.ISBN))
                        throw new UserFriendlyException(L("EmptyISBN"));

                    if(editionInput.ISBN.Length != 10 && editionInput.ISBN.Length != 13)
                        throw new UserFriendlyException(L("InvalidISBNLength", editionInput.ISBN));

                    if (!System.Text.RegularExpressions.Regex.IsMatch(editionInput.ISBN, @"^\d+$"))
                        throw new UserFriendlyException(L("InvalidISBNFormat", editionInput.ISBN));

                    var publishedDate = editionInput.PublishedDate ?? DateTime.Now;
                    if (publishedDate > DateTime.Now.AddYears(1))
                        throw new UserFriendlyException(L("InvalidPublishedDateFuture"));
                   
                    if (string.IsNullOrWhiteSpace(editionInput.Publisher) || editionInput.Publisher.Length > 200)
                        throw new UserFriendlyException(L("PublisherIsRequiredOrTooLong"));

                    if (!Enum.IsDefined(typeof(BookConsts.Format), editionInput.Format))
                        throw new UserFriendlyException(L("InvalidBookFormat"));

                    edition.Format = editionInput.Format;
                    edition.Publisher = editionInput.Publisher;
                    edition.PublishedDate = editionInput.PublishedDate ?? DateTime.Now;
                    edition.ISBN = editionInput.ISBN;

                    await _bookEditionRepository.UpdateAsync(edition);
                }
                else
                {
                    // New edition
                    var existingEdition = await _bookEditionRepository
                        .FirstOrDefaultAsync(e => e.ISBN == editionInput.ISBN);

                    if (existingEdition != null)
                    {
                        throw new UserFriendlyException(L("DuplicateISBN", editionInput.ISBN));
                    }
                    if (string.IsNullOrWhiteSpace(editionInput.ISBN))
                        throw new UserFriendlyException(L("EmptyISBN"));

                    if (editionInput.ISBN.Length != 10 && editionInput.ISBN.Length != 13)
                        throw new UserFriendlyException(L("InvalidISBNLength", editionInput.ISBN));

                    if (!System.Text.RegularExpressions.Regex.IsMatch(editionInput.ISBN, @"^\d+$"))
                        throw new UserFriendlyException(L("InvalidISBNFormat", editionInput.ISBN));

                    var publishedDate = editionInput.PublishedDate ?? DateTime.Now;
                    if (publishedDate > DateTime.Now.AddYears(1))
                        throw new UserFriendlyException(L("InvalidPublishedDateFuture"));

                    if (string.IsNullOrWhiteSpace(editionInput.Publisher) || editionInput.Publisher.Length > 200)
                        throw new UserFriendlyException(L("PublisherIsRequiredOrTooLong"));

                    if (!Enum.IsDefined(typeof(BookConsts.Format), editionInput.Format))
                        throw new UserFriendlyException(L("InvalidBookFormat"));


                    edition = new BookEdition(
                        book.Id,
                        editionInput.Format,
                        editionInput.Publisher,
                        editionInput.PublishedDate ?? DateTime.Now,
                        editionInput.ISBN
                    );

                    await _bookEditionRepository.InsertAsync(edition);
                    book.Editions.Add(edition);
                }
            }

            await _bookRepository.UpdateAsync(book);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task<List<SelectListItemDto>> GetBookGenreAsync()
        {
            var L = _localizationManager.GetSource(BookstoreConsts.LocalizationSourceName);

            var list = Enum.GetValues(typeof(BookConsts.Genre))
                .Cast<BookConsts.Genre>()
                .Select(g => new SelectListItemDto
                {
                    Value = ((int)g).ToString(),
                    Text = L.GetString($"Genre_{g}")
                })
                .ToList();

            return await Task.FromResult(list);
        }

        public async Task<List<BookImageDto>> UploadBookImagesAsync([FromForm] int bookId, [FromForm] List<IFormFile> files, [FromForm] bool isCover = false)
        {
            var book = await _bookRepository.FirstOrDefaultAsync(bookId);
            if (book == null)
                throw new UserFriendlyException(L("BookNotFound"));

            if (files == null || !files.Any())
                throw new UserFriendlyException(L("NoFilesProvided"));
            if(files.Count > 10)
                throw new UserFriendlyException(L("MaxTenImagesAllowed"));
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "books", bookId.ToString());
            Directory.CreateDirectory(uploadPath);

            var uploadedImages = new List<BookImageDto>();
            int displayOrder = 0;

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    throw new UserFriendlyException(L("InvalidImageFormat", file.FileName));
                if (!file.ContentType.StartsWith("image/"))
                    throw new UserFriendlyException(L("InvalidImageFileType"));

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/uploads/books/{bookId}/{fileName}";
                var caption = $"{book.Title}_{displayOrder + 1}";

                // Save to DB
                var bookImage = new BookImage(bookId, relativePath, caption, displayOrder: displayOrder, isCover: displayOrder == 0);
                await _bookImageRepository.InsertAsync(bookImage);

                uploadedImages.Add(new BookImageDto
                {
                    BookId = bookId,
                    ImagePath = relativePath,
                    Caption = caption,
                    DisplayOrder = displayOrder,
                    IsCover = displayOrder == 0
                });

                displayOrder++;
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            return uploadedImages;
        }
        public async Task<List<BookImageDto>> GetBookImagesAsync(int bookId)
        {
            var book = await _bookRepository.FirstOrDefaultAsync(bookId);
            if (book == null)
                throw new UserFriendlyException(L("BookNotFound"));

            var images = await _bookImageRepository
                .GetAll()
                .Where(i => i.BookId == bookId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            return images.Select(i => new BookImageDto
            {
                Id = i.Id,
                BookId = i.BookId,
                ImagePath = i.ImagePath,
                Caption = i.Caption,
                DisplayOrder = i.DisplayOrder,
                IsCover = i.IsCover
            }).ToList();
        }
        [Abp.Authorization.AbpAuthorize("Pages.Books.Update")]
        public async Task DeleteBookImages(DeleteBookImagesDto input)
        {
            foreach (var id in input.Ids)
            {
                var image = await _bookImageRepository.FirstOrDefaultAsync(id);
                if (image != null)
                {
                    var path = Path.Combine(_env.WebRootPath, image.ImagePath.TrimStart('/'));
                    if (File.Exists(path)) File.Delete(path);
                    await _bookImageRepository.DeleteAsync(image);
                }
            }
        }

    }
}
