using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Books.Dto
{
    public class ImportBookExcelDto
    {
        [Required(ErrorMessage = "Please select an Excel file.")]
        public IFormFile ExcelFile { get; set; }
    }
}
