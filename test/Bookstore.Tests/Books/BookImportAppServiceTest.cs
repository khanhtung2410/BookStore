using Bookstore.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Tests.Books
{
    public class BookImportAppServiceTest : BookstoreTestBase
    {
        private readonly IBookImportAppService _bookImportAppService;
        public BookImportAppServiceTest()
        {
            _bookImportAppService = Resolve<IBookImportAppService>();
        }
        [Fact]
        public async Task Should_Import_Books_From_Excel()
        {
            // Arrange
            byte[] fileBytes = System.IO.File.ReadAllBytes("~/TestData/BookImportTemplate.xlsx");
            // Act
            await _bookImportAppService.ImportBooksFromExcel(fileBytes);
            // Assert
            // Add assertions to verify that books were imported correctly
            // This could involve checking the database for the expected records
            Console.WriteLine("Books imported successfully from Excel.");
        }
    }
}
